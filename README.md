**NuGet package: [nuget.org/packages/RendleLabs.UnpkgCli](https://www.nuget.org/packages/RendleLabs.UnpkgCli)**

# dotnet unpkg
I got fed up of needing to have Node.js and NPM installed just so I could install
front-end packages like jQuery and Bootstrap. I'm not using Webpack or anything,
and I don't want 100MB of `node_modules` in every project.

So I made a `dotnet` command to do it instead.

## Why should I use it?

Because you're building an ASP.NET Core application which just needs common front-end
packages like Bootstrap, jQuery and Popper.js, and it's going to serve them from a
CDN in Production but with fallback to local files. You're not compiling your
front-end code with Webpack or anything, and you just want an easy way to acquire
those libraries, without needing to install Node.js and NPM or Yarn or Bower,
and without adding a Gulp or Grunt step just to copy the files you actually need
out of `node_modules`.

`unpkg` is written in C#, with no dependency on JavaScript runtimes, so it installs
as a .NET Global Tool. It'll grab the files you need from the same public CDN you can
use for Production &mdash; [unpkg.com](https://unpkg.com) &mdash; and puts them right
into `wwwroot\lib`, where they belong.

## Why shouldn't I use it?

If you are building a complex SPA, with Angular or TypeScript or Webpack or suchlike,
and you've got code that loads packages from `node_modules` using `import` syntax,
then this is not for those projects, and you should use [NPM](https://npmjs.com).
(You could also use Yarn, but that's by Facebook so for all you know it might be
sending copies of your dependency graphs to shady data-mining organisations; be
careful out there.)

## How does it work?

There's a magic CDN called [unpkg.com](https://unpkg.com) that delivers files from
NPM packages. If those packages follow a simple rule, which is to put all their
runtime files into a folder called `dist`, they can be served from **unpkg**.

It also provides metadata about the packages in JSON format, including the `integrity`
hash that you can use in your script tags to make sure you're getting the right data
and the user's connection hasn't been compromised.

`unpkg` uses that metadata to discover the files in the package and download
them right into your `wwwroot/lib` folder.

Sometimes the packages don't have a `dist` folder, in which case `unpkg` will
download pretty much everything.

## Usage

You'll need the .NET Core SDK 2.1 (currently RC1) installed on your machine.

Then you can install the package as a global tool like this:

```bash
$ dotnet tool install -g --version 2.0.0-rc1 RendleLabs.Unpkg
```

Then, from the command line:

```
$ unpkg add vue
```

It supports NPM-namespaced packages:

```
$ unpkg add @aspnet/signalr
```

You can install multiple packages in a single command:

```
$ unpkg add jquery bootstrap popper.js
```

If you want a specific version, use the `@` notation:

```
$ unpkg add bootstrap@3.3.7
```

You can also specify a path within the package, which is a feature I added
specifically for [Bootswatch](https://bootswatch.com) so I could do this:

```
$ unpkg add bootswatch/yeti
```
That just installs the **Yeti** theme within the larger Bootswatch package. If
you just install Bootswatch by itself, you'll get all 20-odd themes.

You can also specify paths with namespaced packages. This is incredibly useful if you
want to install [Rx.js](http://reactivex.io/rxjs/) because it's *huge*, and all you
want is the `global` folder:

```
$ unpkg add @reactivex/rxjs/global
```
And you'll just get the four `<script>`-tag-friendly files that you need, and not the hundreds of Node and Webpack and source files.

Add specific versions using `@` notation, e.g.

```
$ unpkg add jquery@3.3.0
```

Update to latest versions of packages with a single command:

```
$ unpkg update
```

To update specific packages just add their names, e.g.

```
$ unpkg update bootstrap
```

Aliases for commands:

- `add` is also `a`
- `restore` is also `r`
- `update` is also `u`, `up` or `upgrade` because I can never remember which one it is

Override where `unpkg` puts your files instead of `wwwroot` by one of:

- JSON config file in `$HOME/.unpkg/unpkg.config`
- Environment variable `UNPKG_WWWROOT`
- JSON config file in `./unpkg.config`
- `--wwwroot=public`

It's using .NET Core Configuration, so each of those will override the previous ones.

### unpkg.json

The `add` command stores the details about the files it downloaded into a file in the
current directory, `unpkg.json`. Once that's there, you can just run

```
$ unpkg restore
```

to redownload everything, and it remembers the version, too, so it won't sneakily
upgrade you to jQuery 4.0 when you're not looking.

If you can get all your `wwwroot/lib` dependencies using `unpkg`, then you can add
it to your `.gitignore` and save checking all those files in. Just make sure the
`unpkg.json` file is checked in.

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

## Open Source

`unpkg` is open source, and incorporates the work of other open source projects, specifically:

- [UNPKG](https://github.com/unpkg) by Michael Jackson
- [semver](https://github.com/maxhauser/semver) by Max Hauser
- [JSON.NET](https://www.newtonsoft.com/json) by James Newton-King

Thank you to all these creators for their contributions to the open-source ecosystem.
