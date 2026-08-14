# Handoff: Microsoft Login via Easy Auth (multi-tenant, own tenant, domain-checked)

Plan for adding real authentication to AvaFind, replacing the current IP-restriction-only
access control described in the README's "Access control — deliberate v1 choice" section.

## Constraint driving this design

The user cannot register an app in Avanade's corporate Entra ID tenant (security policy).
Solution: register a **multi-tenant** app in the user's **own** tenant (the default Entra ID
directory behind their personal Azure subscription, already used for this project). Avanade
employees sign in with their normal corporate credentials via the multi-tenant "common"
endpoint — Avanade's tenant is never modified or asked to register anything. Avanade's own
Conditional Access/MFA policies apply automatically during that sign-in (out of this app's
control either way).

## Decisions

- **App registration**: multi-tenant ("Accounts in any organizational directory"), created in
  the user's own/personal tenant. NOT in Avanade's tenant.
- **Auth mechanism**: App Service Authentication ("Easy Auth") — zero OAuth/session code
  needed, platform-level gate in front of the whole App Service.
- **Access restriction**: since users self-serve (not individually invited as B2B guests),
  Enterprise-App "assignment required" does not work for external-tenant users here → turn
  assignment off, and instead add a small FastAPI middleware that reads Easy Auth's injected
  `X-MS-CLIENT-PRINCIPAL-NAME` header and rejects (403) any request whose email doesn't end
  with the allowed domain (e.g. `@avanade.com`). This header is platform-set/validated by Easy
  Auth and stripped from external requests, so it is not spoofable by the client.
- **MFA**: not independently enforced by this app; relies entirely on Avanade's own tenant
  enforcing MFA for its users (near-certain for a large org, but outside this app's control).
- **Known risk (accepted)**: Avanade's tenant might block its users from consenting to sign
  into an external/unverified multi-tenant app. Validate with a real Avanade test account
  before relying on this. No workaround from our side if blocked — would need Avanade IT
  involvement regardless.
- Remove the existing IP-restriction access control from the App Service (keep the SQL
  firewall rules for the local import script — unrelated concern).

## Steps

### Phase 1 — Azure Portal + tenant configuration (manual, done by the user)

1. In the user's own tenant: register a new Entra ID app (or let the Easy Auth wizard create
   one) with "Supported account types" = "Accounts in any organizational directory (Any Entra
   ID directory — Multitenant)". Redirect URI
   `https://<app>.azurewebsites.net/.auth/login/aad/callback`.
2. App Service → Authentication → Add identity provider → Microsoft → link the multi-tenant
   registration → "Require authentication" → unauthenticated requests: "HTTP 302 Redirect to
   identity provider".
3. Confirm "Assignment required?" is set to **No** on the app's Enterprise Application (must
   be off for external-tenant self-service sign-in to work at all).
4. Add an App Service application setting `ALLOWED_EMAIL_DOMAIN` = `avanade.com` (or whatever
   domain), so it's configurable without a code change.
5. Test sign-in with one real Avanade account first to confirm Avanade's tenant doesn't block
   external app consent, before rolling out further.
6. **Ordering note**: do this Portal setup before (or in the same change window as) removing
   the Bicep IP restriction in Phase 3, so the app is never briefly open with no access
   control at all.

### Phase 2 — Backend: domain-restriction middleware

*(small amount of app code — the only code needed for auth in this design)*

7. `backend/app/main.py`: add a small `@app.middleware("http")` function that:
   - Reads `request.headers.get("x-ms-client-principal-name")` (Easy Auth-injected,
     platform-validated UPN/email; empty if Easy Auth isn't in front of the app, e.g. local
     dev).
   - If running behind Easy Auth (header present) and the value doesn't case-insensitively
     end with `@{ALLOWED_EMAIL_DOMAIN}`, return a 403 plain-text response explaining the app
     is restricted to that domain.
   - If the header is absent (local dev, no Easy Auth), let the request through unchanged —
     local dev stays unauthenticated as before.
   - Read `ALLOWED_EMAIL_DOMAIN` from env (only meaningful when Easy Auth headers are
     present, so no local-dev impact if unset).

### Phase 3 — Infra: remove IP restriction

*(parallel with Phase 1/2, but don't deploy until Phase 1's Easy Auth config is live)*

8. `infra/resources.bicep` — remove `ipSecurityRestrictionsDefaultAction`,
   `ipSecurityRestrictions`, and `scmIpSecurityRestrictionsUseMain` from the App Service
   `siteConfig`. Keep the `fwClients` SQL firewall rules (still needed for the local import
   script) and the `allowedClientIps` param.
9. Redeploy: `az deployment sub create ... --parameters @infra/main.parameters.json`.

### Phase 4 — Optional frontend UX (no auth logic, just display)

*(independent, can be skipped/deferred)*

10. `frontend/src/App.jsx` — optionally fetch Easy Auth's `/.auth/me` to show the signed-in
    user's email, plus a "Sign out" link to `/.auth/logout?post_logout_redirect_uri=/`.
11. `frontend/src/api.js` — optionally: if a fetch to `/api/*` ever returns 401/403 (session
    expired or domain-rejected), redirect the browser to `/.auth/login/aad` or show a clear
    "not authorized" message instead of a raw error.

### Phase 5 — Docs

12. `README.md` — replace the "Access control — deliberate v1 choice" section: describe the
    multi-tenant Easy Auth model (registered in the user's own tenant, not Avanade's), the
    domain-check middleware, and the MFA/consent-policy caveats.

## Relevant files

- `backend/app/main.py` — add the ~10-line domain-check middleware (only backend change needed)
- `infra/resources.bicep` — drop IP-restriction block
- `frontend/src/App.jsx`, `frontend/src/api.js` — optional signed-in-user UX (Phase 4)
- `README.md` — replace access-control section

## Verification

1. Real Avanade test account: sign-in succeeds, MFA prompted by Avanade's own tenant policy
   (not this app), lands on the app.
2. A Microsoft account/tenant with a different domain successfully authenticates via Easy
   Auth but is rejected with 403 by the app's domain-check middleware (confirms the
   middleware, not just Easy Auth, is doing the domain gating).
3. `curl` locally (no Easy Auth headers) still reaches the app normally — confirms local dev
   is unaffected.
4. After Phase 3 redeploy: confirm the App Service's IP restriction is gone
   (`az webapp config access-restriction show`) and the app is still only reachable by
   authenticated, correct-domain users.
5. `/.auth/me` and `/.auth/logout` work as expected if Phase 4 is implemented.

## Further considerations

1. **Consent-policy risk (open item)**: if Avanade blocks external app consent tenant-wide, no
   client-side workaround exists — would need Avanade IT to allow it, defeating the "no
   registration in their tenant" constraint. Confirm early with a real test account (Phase 1
   step 5) before investing further.
2. MFA is enforced entirely by Avanade's tenant, not this app — if Avanade doesn't require
   MFA for the test user, none will be applied here either; this is inherent to relying on the
   home tenant's own policies.
3. Local dev has no auth at all (Easy Auth is Azure-platform-only) — same as current behavior,
   just documented clearly in the README.
4. Removing the IP restriction also opens the Kudu/SCM deploy endpoint — flag if a separate
   SCM-only IP restriction should be kept instead of full removal.
