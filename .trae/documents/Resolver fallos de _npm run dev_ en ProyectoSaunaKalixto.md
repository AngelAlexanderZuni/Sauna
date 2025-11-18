## Objetivos
- Modernizar `Login` y `Dashboard` con estética limpia inspirada en shadcn/ui.
- Unificar estilos en clases reutilizables con Tailwind (`@layer components`).
- Mejorar accesibilidad, estados de error, foco y responsividad.

## Alcance y Archivos
- Editar `Pages/Auth/Login.cshtml` (estructura y estilos del formulario).
- Editar `Pages/Dashboard.cshtml` (layout, navegación, tarjetas y acciones).
- Editar `wwwroot/css/site.css` para añadir componentes Tailwind personalizados vía `@layer components`.
- No crear archivos nuevos salvo ser imprescindible; reutilizar los existentes.

## Diseño Visual y UX
- Paleta: base en grises, acentos en azul/verde/rojo; sombras suaves y bordes redondeados.
- Tipografía y espaciados consistentes; jerarquía clara (títulos, subtítulos, cuerpo).
- Estados: hover, focus visibles; deshabilitado atenuado; errores con texto y color.
- Responsive: mobile-first con grillas adaptativas.

## Componentes Reutilizables (Tailwind `@layer components`)
- `btn` variantes: `btn-primary`, `btn-secondary`, `btn-ghost`, `btn-destructive`.
- `input`, `label`, `form-control` con estados `:focus`, `aria-invalid`, mensajes de ayuda/error.
- `card` con `card-header`, `card-title`, `card-content`, `card-footer`.
- `badge` (success/warn/error/neutral), `alert` (info/success/error), `nav-link` activo/inactivo.

## Accesibilidad
- Uso de `aria-label`, `aria-describedby` en inputs.
- Focus ring consistente (`focus:ring-2 focus:ring-offset-2`).
- Contrastes AA para texto y fondos; iconos decorativos con `aria-hidden`.

## Implementación Técnica
- Añadir en `wwwroot/css/site.css` sección `@layer components` definiendo clases utilitarias (inspiradas en shadcn), p.ej.:
  - `.btn { @apply inline-flex items-center justify-center rounded-md font-medium transition-colors focus:outline-none; }`
  - Variantes aplicando colores y estados (`hover`, `disabled`).
- Incrustar SVGs inline reutilizables en las páginas (lock, user, mail, dashboard, users, clients, logout).

## Cambios en Login (`Pages/Auth/Login.cshtml`)
- Layout de panel centrado con `card`:
  - Header con ícono `lock` y título "Iniciar sesión".
  - Form: `label`, `input` para usuario y contraseña; botón `btn-primary` de envío.
  - Link secundario (¿Olvidaste tu contraseña?) como `btn-ghost` (placeholder).
  - Mensajes de error (TempData) en `alert-error`.
- Acciones: botón acceso (submit), y enlace a soporte (placeholder) `btn-secondary`.

## Cambios en Dashboard (`Pages/Dashboard.cshtml`)
- Barra superior con branding (SVG) y menú usuario (nombre/rol, logout `btn-ghost`).
- Navegación tipo tabs simple: `Dashboard`, `Clientes`, `Usuarios` con `nav-link` activo.
- Grilla de tarjetas (`card`) para métricas: Total Clientes, Activos, Membresías por vencer, Registros Hoy; cada una con SVG distintivo.
- Sección de accesos rápidos con `btn-primary` y `btn-secondary` (Clientes/Usuarios). 
- Mensaje de bienvenida adaptado al rol; badges informativos.

## SVG Iconos (inline)
- Login: `lock`, `user`, `eye` (toggle mostrar contraseña), `mail`.
- Dashboard: `graph`, `users`, `id-card`, `calendar`, `logout`.
- Clientes: `user-plus`, `list`.
- Estilo: stroke 2px, `currentColor`, accesibles (`aria-hidden="true"`).

## Estados y Validaciones
- Mostrar mensaje de error de autenticación en `Login` desde `TempData` como `alert-error`.
- Inputs con borde y anillo en error (`aria-invalid="true"`) y texto de ayuda.

## Rendimiento
- Reutilizar clases Tailwind; evitar CSS adicional innecesario.
- Inline SVG para reducir dependencias.

## Entregables
- Estilos componentizados en `wwwroot/css/site.css` (`@layer components`).
- Vistas `Login` y `Dashboard` actualizadas con los componentes.
- Íconos SVG incrustados donde corresponda.

## Verificación
- Ejecutar `npm run dev` y revisar:
  - `Login`: estados de foco, error, responsive.
  - `Dashboard`: navegación, tarjetas, botones.
  - Prueba en móvil y desktop.

## Futuras Mejoras (Opcional)
- Modo oscuro con `data-theme` y variantes.
- Componentizar partes repetidas en parciales si crece el proyecto.

¿Confirmas que avance con estos cambios de estilo en `Login` y `Dashboard` (con clases en `site.css`), imitando shadcn/ui sin agregar dependencias? 