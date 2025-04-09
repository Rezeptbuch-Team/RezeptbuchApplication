CREATE TABLE "recipes" (
  "hash" TEXT PRIMARY KEY,
  "is_download" INTEGER,
  "is_published" INTEGER,
  "is_modified" INTEGER,
  "last_published_hash" TEXT,
  "title" TEXT,
  "description" TEXT,
  "image_path" TEXT,
  "cooking_time" TEXT,
  "file_path" TEXT
);

CREATE TABLE "categories" (
  "id" INTEGER PRIMARY KEY,
  "name" TEXT
);

CREATE TABLE "recipe_category" (
  "hash" TEXT,
  "category_id" INTEGER
);

CREATE TABLE "ingredients" (
  "id" INTEGER PRIMARY KEY,
  "name" TEXT
);

CREATE TABLE "recipe_ingredient" (
  "hash" TEXT,
  "ingredient_id" INTEGER
);

ALTER TABLE "recipe_category" ADD FOREIGN KEY ("hash") REFERENCES "recipes" ("hash");

ALTER TABLE "categories" ADD FOREIGN KEY ("id") REFERENCES "recipe_category" ("category_id");

ALTER TABLE "recipe_ingredient" ADD FOREIGN KEY ("hash") REFERENCES "recipes" ("hash");

ALTER TABLE "ingredients" ADD FOREIGN KEY ("id") REFERENCES "recipe_ingredient" ("ingredient_id");
