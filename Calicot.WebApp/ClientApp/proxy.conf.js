const { env } = require('process');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
  env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:58750';

const PROXY_CONFIG = [
  {
    context: [
      "/api",
      "/api/FileUpload",
      "/api/Image",
      "/api/Produits",
      "/api/Users",
      "/authenticate",
      "/signin-google"
   ],
    target: target,
    secure: false,
    headers: {
      Connection: 'Keep-Alive'
    },
    logLevel: 'debug'
  }
];

module.exports = PROXY_CONFIG;
