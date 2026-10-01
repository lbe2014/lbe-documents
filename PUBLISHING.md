# Publicar en NuGet.org

El workflow `.github/workflows/publish.yml` publica un paquete al crear un **release no preliminar** en GitHub. Usa [Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing): no necesita una API key permanente.

`Lbe.Documents` `0.1.0` ya está publicado. Este procedimiento es para versiones posteriores o para configurar otro repositorio; no vuelvas a publicar la misma versión.

## Configuración inicial (una sola vez)

1. Crea o inicia sesión en tu cuenta de [NuGet.org](https://www.nuget.org/).
2. En NuGet.org, abre tu usuario → **Trusted Publishing** → agrega una política de GitHub con:
   - Repository owner: `lbe2014`
   - Repository: `lbe-documents`
   - Workflow file: `publish.yml` (solo el nombre)
   - Environment: vacío
3. En GitHub, abre el repositorio → **Settings → Secrets and variables → Actions → Variables** y crea `NUGET_USER` con tu **nombre de usuario de NuGet.org**, no tu correo ni una contraseña.
4. Comprueba que la etiqueta del release coincida con `<Version>` del proyecto, precedida por `v`. Para cada actualización, elige primero una versión nueva y usa la etiqueta equivalente.

## Lanzar una versión

1. Comprueba que CI haya pasado y revisa el paquete, el README, las licencias y los ejemplos.
2. En GitHub, abre **Releases → Draft a new release**, crea desde `main` la etiqueta `v` seguida de la nueva versión del proyecto, revisa la descripción y publica el release. No lo marques como prerelease si quieres que este workflow lo publique en NuGet.
3. Revisa **Actions → Publish to NuGet**. El job prueba, empaqueta, obtiene una credencial temporal y publica.
4. Confirma que la nueva versión aparezca en `https://www.nuget.org/packages/Lbe.Documents/` y que se pueda instalar con `dotnet add package Lbe.Documents --version <nueva-versión>`.

La publicación es difícil de revertir: NuGet.org no permite sobrescribir una versión existente. El README integrado en el paquete `0.1.0` conserva el texto del momento de su publicación; cambiar este repositorio no cambia ese paquete. Si quieres corregir el README que muestra NuGet.org, tendrás que incluirlo en una versión nueva. No subas claves a GitHub ni al repositorio.
