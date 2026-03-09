const form = document.getElementById('search-form');
const queryInput = document.getElementById('query');

form.addEventListener('submit', function(event) {
    event.preventDefault();
    const query = queryInput.value.trim();

    if (query.length === 0) {
        queryInput.focus();
        return;
    }

    const targetUrl = 'https://duckduckgo.com/?q=' + encodeURIComponent(query);
    window.location.href = targetUrl;
});