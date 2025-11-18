## Diagnóstico Rápido
- `package.json` usa `dotnet watch` contra `ProyectoSaunaKalixto.Web.csproj` (`package.json`:6-9).
- Ese proyecto apunta a `net9.0` (`ProyectoSaunaKalixto.Web.csproj`:4). Si tu SDK es `net8`, esto falla con NETSDK1045.
- Existe otro proyecto `ProyetoSaunaKalixto.csproj` en `net8.0` (`ProyetoSaunaKalixto.csproj`:4). La mezcla de proyectos puede confundir el arranque.
- Hay `TailwindCSS.targets` que intenta lanzar `npm` durante Build/Watch (`TailwindCSS.targets`:2-8). Esto puede duplicar procesos si además usamos `npm run dev` con `watch:css`.
- `postcss.config.js` declara `autoprefixer`, pero no está en `devDependencies` del `package.json`. Puede provocar errores de módulo faltante.

## Plan de Corrección
1. Unificar el proyecto de arranque
   - Opción A (recomendada): Cambiar el script `dev` para usar `ProyetoSaunaKalixto.csproj` (`net8.0`).
   - Opción B: Bajar `ProyectoSaunaKalixto.Web.csproj` a `net8.0` para que ambos coincidan.
2. Asegurar dependencias Node
   - Añadir `autoprefixer` y `postcss` a `devDependencies` y ejecutar `npm install`.
3. Evitar duplicidad de watchers
   - Mantener `npm run dev` como única fuente de watch para CSS.
   - Desactivar/retirar el `TailwindWatch` del `TailwindCSS.targets` (dejar solo el build en compilación si se desea).
4. HTTPS de desarrollo
   - Si aparece error de certificado, confiar el certificado de desarrollo con `dotnet dev-certs https --trust`.
5. Verificación
   - Ejecutar `npm install`.
   - Ejecutar `npm run dev` y validar que:
     - `dotnet watch` arranca sin error de SDK.
     - `tailwindcss --watch` compila `wwwroot/css/output.css` sin errores de PostCSS.
   - Abrir `https://localhost:5001` y navegar a `/Auth/Login` y `/Clientes`.

## Cambios Propuestos (mínimos)
- `package.json`: Ajustar `scripts.dev` para apuntar al proyecto `net8` O ajustar `TargetFramework` del proyecto Web a `net8.0`.
- `package.json`: Añadir `autoprefixer` y `postcss` a `devDependencies`.
- `TailwindCSS.targets`: Desactivar el target `TailwindWatch` para evitar procesos duplicados.

¿Confirmas que proceda con estos ajustes para que `npm run dev` funcione correctamente en tu entorno?