# CryptoGuide frontend

React and TypeScript frontend built with Vite. CryptoGuide is an educational cryptocurrency app with a simulated portfolio; it does not execute real trades.

CG-002 is in progress. The app currently displays the Vite starter. The CryptoGuide landing page and responsive navigation are still to be implemented. Backend integration belongs to CG-003; the backend is not required to run this frontend yet.

## Prerequisites

Install Node.js and npm. The installed Vite version requires Node.js `^20.19.0 || >=22.12.0`; use a supported Node.js LTS release satisfying that range.

Check your installation:

```powershell
node --version
npm --version
```

On Windows PowerShell, if execution policy blocks `npm.ps1`, use `npm.cmd` in place of `npm` in the commands below. No execution-policy change is needed.

## Install and run

From the repository root:

```powershell
cd frontend
npm ci
npm run dev
```

`npm ci` installs the versions recorded in `package-lock.json`. Use `npm install` when intentionally adding or updating dependencies, and commit the resulting lockfile changes.

Open the URL printed by Vite, normally `http://localhost:5173`. If the port is occupied, Vite may select another. Saving source changes updates the browser through hot module replacement. Stop the server with `Ctrl+C`.

## Available commands

Run these from `frontend/`:

| Command | Purpose |
|---|---|
| `npm run dev` | Start the development server |
| `npm run lint` | Run ESLint |
| `npm run build` | Check TypeScript with `tsc -b`, then create a production bundle in `dist/` |
| `npm run preview` | Serve an existing production build locally for inspection |

To inspect the production build:

```powershell
npm run build
npm run preview
```

Use the URL printed by the preview server. Preview is for local verification, not production hosting.

## Linting and TypeScript

ESLint is configured in `eslint.config.js` for TypeScript and TSX files. It includes:

- Recommended JavaScript and TypeScript rules.
- React Hooks rules.
- React Refresh rules for Vite development.

`dist/` is excluded from linting. ESLint checks coding patterns; TypeScript checks types. Run both before committing:

```powershell
npm run lint
npm run build
```

The current ESLint configuration uses `tseslint.configs.recommended`, without type-aware lint rules. If needed later, follow the [typescript-eslint typed linting guide](https://typescript-eslint.io/getting-started/typed-linting/) to enable `recommendedTypeChecked` and its parser configuration. Extra React lint plugins and React Compiler are not currently configured.

**CG-002 requirement still to address:** TypeScript `strict` is not explicitly enabled in either `tsconfig.app.json` or `tsconfig.node.json`, and the root configuration does not enable it. Enable `"strict": true` in the application configuration and preferably the tooling configuration, then resolve any resulting errors. Existing unused-variable checks do not enable strict mode.

## Project files

| Path | Purpose |
|---|---|
| `src/main.tsx` | React application entry point |
| `src/App.tsx` | Current application component |
| `src/` | Application components, styles and imported assets |
| `public/` | Static assets served directly |
| `vite.config.ts` | Vite configuration and React plugin |
| `tsconfig.app.json` | TypeScript settings for application code |
| `tsconfig.node.json` | TypeScript settings for Vite tooling |
| `eslint.config.js` | ESLint rules |

React Router and automated tests are not installed/configured yet. For CG-002, manually check navigation, narrow-screen layout and keyboard focus after implementing the landing shell.

Keep `node_modules/`, `dist/` and credentials out of Git. Frontend environment variables exposed through Vite's `VITE_` prefix are visible to browser users; never put provider keys or other secrets in them.

Use the shared [conventional commit policy](../docs/Conventional_Commits.md) and [product backlog](../docs/CryptoGuide_Product_Backlog.md) for ticket scope.
