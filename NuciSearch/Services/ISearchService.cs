namespace NuciSearch.Services
{
    /// <summary>
    /// Builds destination URLs for user search queries.
    /// </summary>
    public interface ISearchService
    {
        /// <summary>
        /// Builds a destination URL for the supplied query and search type.
        /// </summary>
        /// <param name="rawQuery">The user-entered query.</param>
        /// <param name="searchType">The selected search type.</param>
        /// <returns>The destination URL, or an empty string for an empty query.</returns>
        string GetSearchUrl(string rawQuery, string searchType);
    }
}
