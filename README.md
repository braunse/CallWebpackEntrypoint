What is this?
=============

A small library to load all assets for a webpack-packaged entrypoint in an ASP.NET Core application
with razor pages.

Why do I need this?
===================

Because keeping track of webpack's chunk splitting behaviour, the weird file names and the 
hashes is a pain in the butt.
This project has a companion webpack plugin (https://github.com/braunse/webpack-entrypoint-list-plugin)
that will write a JSON file of all scripts and styles, which endpoints they are needed for,
and their precomputed SRI hashes.

How to enable
=============

 1. Install the webpack plugin
 2. Embed the resulting resources in your assembly
 3. ```
      services.AddWebpackEntrypoints(typeof(Startup).Assembly, 
          urlPrefix: "/assets",
          pathToJson: "ASSEMBLY_SCOPE_HERE.JSON_FILE_NAME");
    ```

