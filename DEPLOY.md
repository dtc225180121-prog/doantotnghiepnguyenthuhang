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
