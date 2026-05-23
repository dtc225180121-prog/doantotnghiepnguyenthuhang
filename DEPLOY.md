# Deploy English Hub

## 1. Deploy backend + PostgreSQL on Render

1. Push this repository to GitHub.
2. Open Render.
3. Choose **New** -> **Blueprint**.
4. Connect the GitHub repository.
5. Render will read `render.yaml` and create:
   - `aoe-backend`
   - `aoe-db`
6. Add `Gemini__ApiKey` if AI features are needed.
7. Wait until backend deploy completes.
8. Open:

```text
https://YOUR_RENDER_BACKEND_URL/healthz
https://YOUR_RENDER_BACKEND_URL/healthz/db
```

Both should return success.

## 2. Connect Vercel frontend to backend

If the backend URL is different from `https://aoe-backend-3.onrender.com`, set this Vercel environment variable:

```text
API_ORIGIN=https://YOUR_RENDER_BACKEND_URL
```

Then redeploy Vercel:

```sh
npx vercel deploy --prod --yes
```

## 3. Test online

Open:

```text
https://english-hub-aoe.vercel.app
```

Register and login should work when `/healthz/db` reports that the database is connected.

## 4. GitHub Actions CI/CD

This repository also includes automated CI/CD in [`.github/workflows/ci-cd.yml`](.github/workflows/ci-cd.yml).

- On every pull request to `main`, it builds the backend and the frontend.
- On every push to `main`, it builds first and then triggers deploy hooks if they are configured.

Required GitHub secrets:

- `RENDER_DEPLOY_HOOK_URL` for the backend deploy hook.
- `VERCEL_DEPLOY_HOOK_URL` for the frontend deploy hook.
- `API_ORIGIN` if you want the frontend build to inject a different backend URL.

If you do not set the deploy hook secrets yet, the CI build still runs and the deploy job skips safely.
