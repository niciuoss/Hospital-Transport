import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  output: 'standalone',
  eslint: {
    // Ignora erros de ESLint durante o build em produção
    ignoreDuringBuilds: true,
  },
  typescript: {
    // Ignora erros de TypeScript durante o build em produção
    ignoreBuildErrors: true,
  },
};

export default nextConfig;
