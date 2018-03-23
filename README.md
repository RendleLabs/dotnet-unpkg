[nuget.org/packages/RendleLabs.UnpkgCli](https://www.nuget.org/packages/RendleLabs.UnpkgCli)

# dotnet unpkg
I got fed up of needing to have Node.js and NPM installed just so I could install
front-end packages like jQuery and Bootstrap. I'm not using Webpack or anything,
so I don't want 100MB of `node_modules` in every project.

So I made a dotnet command to do it instead.

## How does it work?

There's a magic CDN called [unpkg.com](https://unpkg.com) that delivers files from
NPM packages. If those packages follow a simple rule, which is to put all their
runtime files into a folder called `dist`, they can be served from **unpkg**.

It also provides metadata about the packages in JSON format, including the `integrity`
hash that you can use in your script tags to make sure you're getting the right data
and the user's connection hasn't been compromised.

`dotnet-unpkg` uses that metadata to discover the files in the package and download
them right into your `wwwroot/lib` folder.

Sometimes the packages don't have a `dist` folder, in which case `dotnet unpkg` will download pretty much everything.

## Usage

Install the package into your project as a tool reference:

```xml
  <ItemGroup>
    <DotNetCliToolReference Include="RendleLabs.UnpkgCli" Version="1.0.0-*" />
  </ItemGroup>
```

Then, from the command line:

```
$ dotnet unpkg add vue
```

You can install multiple packages in a single command:

```
$ dotnet unpkg add jquery bootstrap popper.js
```

If you want a specific version, use the `@` notation:

```
$ dotnet unpkg add bootstrap@3.3.7
```

You can also specify a path within the package, which is a feature I added
specifically for [Bootswatch](https://bootswatch.com) so I could do this:

```
$ dotnet unpkg add bootswatch/yeti
```
That just installs the **Yeti** theme within the larger Bootswatch package. If
you just install Bootswatch by itself, you'll get all 20-odd themes.

### unpkg.json

The `add` command stores the details about the files it downloaded into a file in the current directory, `unpkg.json`. Once that's there, you can just run

```
dotnet unpkg restore
```

to redownload everything, and it remembers the version, too, so it won't sneakily
upgrade you to jQuery 4.0 when you're not looking.

If you can get all your `wwwroot/lib` dependencies using `unpkg`, then you can add
it to your `.gitignore` and save checking all those files in. Just make sure the `unpkg.json` file is checked in.

Once you've got a package installed, the `restore` command will just use the info
from `unpkg.json`, so if there are files you don't want you can edit it and remove
them. Saving should be non-destructive. If you run `add` again for a package that
is already in `unpkg.json`, it will be overwritten with whatever version it finds
on the CDN.

### Integrity hashes

The other thing that goes into the `unpkg.json` file is the integrity hash for each
file. You should use this in your `<script>` and `<link>` tags, like this:

#### Example script tag

```html
<script src="https://unpkg.com/jquery@3.3.1/dist/jquery.slim.js"
  asp-fallback-src="~/lib/jquery/jquery.slim.min.js"
  asp-fallback-test="window.jQuery"
  crossorigin="anonymous"
  integrity="sha384-q8i/X+965DzO0rT7abK41JStQIAqVgRVzpbzo5smXKp4YfRvH+8abtTE1Pi6jizo">
</script>
```

#### Example link tag

```html
<link href="https://unpkg.com/bootswatch@4.0.0/dist/darkly/bootstrap.min.css"
  rel="stylesheet"
  integrity="sha384-p8bH4RlA/kdF4wbAoep+/6VeOQI2IAWa9zLjTIQbQLv2FuCjX/W/FkdYdeKISDvK"
  crossorigin="anonymous"
  asp-fallback-href="~/bootstrap4/bootstrap/bootswatch.min.css"
  asp-fallback-test-class="sr-only"
  asp-fallback-test-property="position"
  asp-fallback-test-value="absolute" />
```

**Note to self:** maybe generate these tags, either as an extra command or
into another file somewhere?

