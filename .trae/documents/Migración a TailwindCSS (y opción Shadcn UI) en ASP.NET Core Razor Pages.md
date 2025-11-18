**Objetivo**
- Ejecutar Tailwind en modo watch y la app ASP.NET Core en la misma terminal.

**Implementación**
- Añadir `concurrently` como devDependency.
- Actualizar `package.json` con un script `dev` que lance ambos procesos:
  - `"dev": "concurrently -n web,css -c green,cyan \"dotnet watch run\" \"npm run watch:css\""`
- Mantener los scripts existentes (`watch:css`, `build:css`).

**Uso**
- Desarrollo: ejecutar `npm run dev` para iniciar la app (`dotnet watch run`) y el watcher de Tailwind simultáneamente en la misma terminal, con logs etiquetados.
- Publicación: `dotnet publish` ya integra el build de Tailwind vía MSBuild (sin pasos extra).

**Notas**
- `concurrently` funciona bien en Windows PowerShell y colorea/etiqueta logs (`web` y `css`).
- Si prefieres evitar dependencias, podría usarse un script PowerShell con `Start-Job`, pero la experiencia de logs y cancelación es inferior.

¿Confirmas que añada `concurrently` y configure el script `dev` para que luego puedas usar `npm run dev`?