## Objetivos
- Usar la paleta OKLCH proporcionada para TODO el sitio (Login, Dashboard, Clientes, Usuarios).
- Hacer Login ancho moderado en pantallas grandes (sin estirarse).
- Hacer Dashboard/Clientes/Usuarios responsivos y aprovechar pantallas grandes (no pequeños al centro).

## Cambios por Archivo
### `wwwroot/css/site.css`
- Sustituir `:root` y `.dark` por tu paleta OKLCH (exacta).
- Vincular `@layer components` a variables: `btn*`, `input`, `label`, `form-*`, `card*`, `nav-link*`, `alert*`, `badge*`.
- Usar `--radius` para bordes; `--background`, `--foreground`, `--primary`, `--secondary`, `--accent`, `--destructive`, `--border`, `--input`, `--ring`.

### `Pages/Auth/Login.cshtml`
- Contenedor: `w-full max-w-md sm:max-w-lg lg:max-w-xl` (evitar `xl:max-w-2xl`) para no estirarse.
- Fondo y texto con `var(--background)` y `var(--foreground)`; usar componentes `.card`, `.input`, `.btn-primary`.

### `Pages/Dashboard.cshtml`
- Contenedor: `max-w-screen-2xl mx-auto px-6 lg:px-8 xl:px-12` para pantallas grandes.
- Grilla métricas: `grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4`.
- Navegación con `nav-link` activo (usa `--primary`) e inactivo; tarjetas `card` con `--radius`.

### `Pages/Clientes/*.cshtml` y `Pages/Usuarios/*.cshtml`
- Index: envolver bloques en `card`, acciones con `btn-*`, buscador con `input`.
- Create/Edit/Delete: formularios con `label`+`input`, errores `form-error`, acciones `btn-primary/secondary`.
- Aplicar colores desde variables para coherencia.

## Paleta Global (OKLCH)
- Insertar exactamente las variables que enviaste en `:root` y `.dark`.

## Responsividad
- Login centrado con ancho moderado.
- Dashboard amplio y denso en `lg/xl`; listas y formularios adaptables.

## Verificación
- Ejecutar `npm run dev`; revisar Login y Dashboard en monitores grandes.
- Validar Clientes/Usuarios en mobile/desktop y modo `.dark`.

¿Confirmas aplicar estos cambios en `site.css`, Login, Dashboard, Clientes y Usuarios ahora?