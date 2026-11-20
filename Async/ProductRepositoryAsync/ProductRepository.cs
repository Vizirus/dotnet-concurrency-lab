using System.Globalization;

namespace ProductRepositoryAsync;

/// <summary>
/// Represents a product storage service and provides a set of methods for managing the list of products.
/// </summary>
public class ProductRepository : IProductRepository
{
    private readonly string productCollectionName;
    private readonly IDatabase database;

    public ProductRepository(string productCollectionName, IDatabase database)
    {
        this.productCollectionName = productCollectionName;
        this.database = database;
    }

    public async Task<Product> GetProductAsync(int productId)
    {
        await this.CheckCollectionAsync(productId);
        var res = await this.database.GetCollectionElementAsync(this.productCollectionName, productId, out IDictionary<string, string> data);
        HandleOperationResult(res);

        return new Product
        {
            Id = productId,
            Name = data["name"],
            Category = data["category"],
            UnitPrice = decimal.Parse(data["price"], CultureInfo.InvariantCulture),
            UnitsInStock = int.Parse(data["in-stock"], CultureInfo.InvariantCulture),
            Discontinued = bool.Parse(data["discontinued"]),
        };
    }

    public async Task<int> AddProductAsync(Product product)
    {
        var res = await this.database.IsCollectionExistAsync(this.productCollectionName, out bool status);
        HandleOperationResult(res);
        res = await this.database.CreateCollectionAsync(this.productCollectionName);
        if (res == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }

        res = await this.database.GenerateIdAsync(this.productCollectionName, out int id);
        HandleOperationResult(res);
        ValidateValues(product);
        res = await this.database.InsertCollectionElementAsync(this.productCollectionName, id, ConvertToDictionary(product));
        HandleOperationResult(res);
        return id;
    }

    public async Task RemoveProductAsync(int productId)
    {
        await this.CheckCollectionAsync(productId);
        var res = await this.database.DeleteCollectionElementAsync(this.productCollectionName, productId);
        HandleOperationResult(res);
    }

    public async Task UpdateProductAsync(Product product)
    {
        await this.CheckCollectionAsync(product.Id);
        ValidateValues(product);
        var res = await this.database.UpdateCollectionElementAsync(this.productCollectionName, product.Id, ConvertToDictionary(product));
        HandleOperationResult(res);
    }

    private static void ValidateValues(Product product)
    {
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            throw new ArgumentException("Argument Name is whitespace or empty!", nameof(product));
        }

        if (string.IsNullOrWhiteSpace(product.Category))
        {
            throw new ArgumentException("Argument Category is whitespace or empty!", nameof(product));
        }

        if (product.UnitPrice < 0)
        {
            throw new ArgumentException("The price must be 0 or above!", nameof(product));
        }

        if (product.UnitsInStock < 0)
        {
            throw new ArgumentException("The number of units must be 0 or above!", nameof(product));
        }
    }

    private static void HandleOperationResult(OperationResult result)
    {
        if (result == OperationResult.ConnectionIssue)
        {
            throw new DatabaseConnectionException();
        }
        else if (result != OperationResult.Success)
        {
            throw new RepositoryException();
        }
    }

    private static IDictionary<string, string> ConvertToDictionary(Product product)
    {
        Dictionary<string, string> map = new Dictionary<string, string>
        {
            { "name", product.Name },
            { "category", product.Category },
            { "price", product.UnitPrice.ToString(CultureInfo.InvariantCulture) },
            { "in-stock", product.UnitsInStock.ToString(CultureInfo.InvariantCulture) },
            { "discontinued", product.Discontinued.ToString(CultureInfo.InvariantCulture) },
        };

        return map;
    }

    private async Task CheckCollectionAsync(int productId)
    {
        OperationResult res = await this.database.IsCollectionExistAsync(this.productCollectionName, out bool exits);
        HandleOperationResult(res);

        if (!exits)
        {
            throw new CollectionNotFoundException();
        }

        res = await this.database.IsCollectionElementExistAsync(this.productCollectionName, productId, out bool exists);
        HandleOperationResult(res);

        if (!exists)
        {
            throw new ProductNotFoundException();
        }
    }
}
