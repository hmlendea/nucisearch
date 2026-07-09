namespace NuciSearch.Services
{
    public interface ISearchService
    {
        string GetSearchUrl(string rawQuery, string searchType);
    }
}
