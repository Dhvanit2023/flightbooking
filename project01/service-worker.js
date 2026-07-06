const CACHE_NAME = 'aspnet-pwa-v1';
const urlsToCache = [
    '/',
    '/Default.aspx',
    '/Styles/site.css',
    '/Scripts/site.js',
    '/Photos/icon-192.png',  // Correct path if inside a folder
    '/Photos/icon-512.png'   // Correct path if inside a folder
];

// Install event
self.addEventListener('install', event => {
    event.waitUntil(
        caches.open(CACHE_NAME).then(cache => {
            console.log('Caching app shell...');
            return cache.addAll(urlsToCache);
        })
    );
});

// Fetch event (serve cached files)
self.addEventListener('fetch', event => {
    event.respondWith(
        caches.match(event.request).then(response => {
            return response || fetch(event.request);
        })
    );
});

// Activate event (clear old cache)
self.addEventListener('activate', event => {
    event.waitUntil(
        caches.keys().then(keys => {
            return Promise.all(
                keys.filter(k => k !== CACHE_NAME)
                    .map(k => caches.delete(k))
            );
        })
    );
});
