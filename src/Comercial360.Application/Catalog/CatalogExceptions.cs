namespace Comercial360.Application.Catalog;

public sealed class DuplicateCatalogValueException(string message) : Exception(message);
