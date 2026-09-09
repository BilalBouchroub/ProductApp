# SMART PRODUCT — configuration et architecture

SMART PRODUCT est le module conversationnel intégré de ProductApp. Il est accessible aux trois rôles authentifiés et fonctionne en lecture seule sur les données métier.

## Configuration

Configurer les secrets uniquement sur le backend :

```dotenv
SMART_PRODUCT_PROVIDER=Gemini
GEMINI_API_KEY=...
GEMINI_MODEL=gemini-3.6-flash
GEMINI_EMBEDDING_MODEL=gemini-embedding-001
```

Le fichier `backend/.env` est chargé au démarrage : redémarrez le backend
après toute modification. OpenAI reste disponible comme repli avec
`SMART_PRODUCT_PROVIDER=OpenAI` et les variables `OPENAI_*`.

La clé ne doit jamais être placée dans `frontend`, `appsettings.json`, Git ou les logs. Les autres limites sont dans `SmartProduct` dans `backend/src/ProductApp.Api/appsettings.json`.

## Mise en service

```powershell
cd backend
dotnet ef database update --project src/ProductApp.Infrastructure --startup-project src/ProductApp.Api
dotnet run --project src/ProductApp.Api

cd ../frontend
npm run dev
```

Ouvrir `/ai`. Les pages Produit et Expérience peuvent fournir `productId` ou `experimentId` dans l’URL afin d’attacher le contexte métier à une nouvelle conversation.

## Sécurité

- JWT obligatoire et vérification du propriétaire pour chaque conversation, message, pièce jointe et chunk.
- Outils ProductApp en lecture seule ; aucun SQL généré ou exécuté par le modèle.
- Autorisations décidées dans ASP.NET Core, jamais par le fournisseur IA.
- Pièces jointes stockées hors de `wwwroot`, nom aléatoire, extension/MIME/taille contrôlés.
- Documents et données métier placés dans une zone de données non fiable du prompt.
- Réponses ancrées sur les données ProductApp avec sources internes.
- Limitation par utilisateur à 12 générations par minute.

## Documents

TXT, Markdown, CSV, DOCX et XLSX sont extraits et indexés. Les PDF contenant du texte simple sont pris en charge ; les PDF scannés nécessitent un futur adaptateur OCR. Les images sont validées et stockées, mais l’indexation vision est laissée à un futur extracteur. Les embeddings du fournisseur sélectionné sont utilisés lorsque sa clé est configurée ; la recherche lexicale contrôlée sert de repli.

## Fournisseurs IA

Gemini utilise `streamGenerateContent` en SSE et `embedContent` avec
`gemini-embedding-001`. Le frontend ne reçoit jamais la clé API.

OpenAI utilise l’API Responses avec `stream: true` et `store: false`. Les deux fournisseurs sont derrière `IAiProvider`, ce qui permet un remplacement futur par Azure OpenAI ou un modèle local.

## Limites intentionnelles de la première version

Le module ne modifie aucune donnée produit, expérience ou commerciale. La génération PDF/Word d’un rapport reste un adaptateur futur : SMART PRODUCT prépare déjà le contenu structuré et sourcé nécessaire à cet export.
