const form = document.getElementById("search-form");
const queryInput = document.getElementById("query");

function runSearch(rawQuery) {
    const query = rawQuery.trim();

    if (!query) {
        queryInput.focus();
        return;
    }

    const targetUrl = "https://duckduckgo.com/?q=" + encodeURIComponent(query);
    window.location.href = targetUrl;
}

form.addEventListener("submit", function (event) {
    event.preventDefault();
    runSearch(queryInput.value);
});

window.addEventListener("DOMContentLoaded", function () {
    const params = new URLSearchParams(window.location.search);
    const queryFromUrl = params.get("q");

    if (queryFromUrl && queryFromUrl.trim()) {
        queryInput.value = queryFromUrl;
        runSearch(queryFromUrl);
    }
});