# Publicar en NuGet.org

El workflow `.github/workflows/publish.yml` publica un paquete al crear un **release no preliminar** en GitHub. Usa [Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing): no necesita una API key permanente.

## Configuración inicial (una sola vez)

1. Crea o inicia sesión en tu cuenta de [NuGet.org](https://www.nuget.org/).
2. En NuGet.org, abre tu usuario → **Trusted Publishing** → agrega una política de GitHub con:
   - Repository owner: `lbe2014`
   - Repository: `lbe-documents`
   - Workflow file: `publish.yml` (solo el nombre)
   - Environment: vacío
3. En GitHub, abre el repositorio → **Settings → Secrets and variables → Actions → Variables** y crea `NUGET_USER` con tu **nombre de usuario de NuGet.org**, no tu correo ni una contraseña.
4. Comprueba que la etiqueta del release sea `v0.1.0`, igual a `<Version>0.1.0</Version>` del proyecto. Para una actualización, cambia primero la versión del proyecto y usa la etiqueta equivalente.

## Lanzar una versión

1. Comprueba que CI haya pasado y revisa el paquete, el README, las licencias y los ejemplos.
2. En GitHub, abre **Releases → Draft a new release**, crea la etiqueta `v0.1.0` desde `main`, revisa la descripción y publica el release. No lo marques como prerelease.
3. Revisa **Actions → Publish to NuGet**. El job prueba, empaqueta, obtiene una credencial temporal y publica.
4. Confirma que la versión aparezca en `https://www.nuget.org/packages/Lbe.Documents/` y que se pueda instalar con `dotnet add package Lbe.Documents --version 0.1.0`.

La publicación es difícil de revertir: NuGet.org no permite sobrescribir una versión existente. Corrige cualquier problema con una versión nueva. No subas claves a GitHub ni al repositorio.
