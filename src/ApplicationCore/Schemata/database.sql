CREATE TABLE "app_info" (
  "key"   TEXT PRIMARY KEY,
  "value" TEXT NOT NULL
);

CREATE TABLE "recipes" (
  "hash" TEXT PRIMARY KEY,
  "is_download" INTEGER,
  "is_published" INTEGER,
  "is_modified" INTEGER,
  "last_published_hash" TEXT,
  "title" TEXT,
  "description" TEXT,
  "image_path" TEXT,
  "cooking_time" INTEGER,
  "file_path" TEXT
);

CREATE TABLE "categories" (
  "id" INTEGER PRIMARY KEY,
  "name" TEXT,
  FOREIGN KEY ("id") REFERENCES "recipe_category" ("category_id")
);

CREATE TABLE "recipe_category" (
  "hash" TEXT,
  "category_id" INTEGER,
  FOREIGN KEY ("hash") REFERENCES "recipes" ("hash")
);

CREATE TABLE "ingredients" (
  "id" INTEGER PRIMARY KEY,
  "name" TEXT,
  FOREIGN KEY ("id") REFERENCES "recipe_ingredient" ("ingredient_id")
);

CREATE TABLE "recipe_ingredient" (
  "hash" TEXT,
  "ingredient_id" INTEGER,
  FOREIGN KEY ("hash") REFERENCES "recipes" ("hash")
);
