import { defineConfig } from "@hey-api/openapi-ts";

export default defineConfig({
  input: "./openapi.json",
  output: {
    path: "src/api",
    format: "prettier",
  },
  plugins: [
    "@hey-api/typescript",
    {
      name: "@hey-api/sdk",
      asClass: false,
    },
    {
      name: "@hey-api/client-fetch",
      bundle: true,
      baseUrl: "https://localhost:7073",
    },
    {
      name: "@tanstack/react-query",
      queryOptions: true,
      mutationOptions: true,
    },
  ],
});
