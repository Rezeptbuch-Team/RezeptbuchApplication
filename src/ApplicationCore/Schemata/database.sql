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
  "name" TEXT
);

CREATE TABLE "recipe_category" (
  "hash" TEXT NOT NULL,
  "category_id" INTEGER NOT NULL,
  PRIMARY KEY ("hash", "category_id"),
  FOREIGN KEY ("hash") REFERENCES "recipes" ("hash"),
  FOREIGN KEY ("category_id") REFERENCES "categories" ("id")
);

CREATE TABLE "ingredients" (
  "id" INTEGER PRIMARY KEY,
  "name" TEXT
);

CREATE TABLE "recipe_ingredient" (
  "hash" TEXT NOT NULL,
  "ingredient_id" INTEGER NOT NULL,
  PRIMARY KEY ("hash", "ingredient_id"),
  FOREIGN KEY ("hash") REFERENCES "recipes" ("hash"),
  FOREIGN KEY ("ingredient_id") REFERENCES "ingredients" ("id")
);
