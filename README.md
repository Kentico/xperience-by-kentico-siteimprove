# Xperience by Kentico Siteimprove

[![Kentico Labs](https://img.shields.io/badge/Kentico_Labs-grey?labelColor=orange&logo=data:image/svg+xml;base64,PHN2ZyBjbGFzcz0ic3ZnLWljb24iIHN0eWxlPSJ3aWR0aDogMWVtOyBoZWlnaHQ6IDFlbTt2ZXJ0aWNhbC1hbGlnbjogbWlkZGxlO2ZpbGw6IGN1cnJlbnRDb2xvcjtvdmVyZmxvdzogaGlkZGVuOyIgdmlld0JveD0iMCAwIDEwMjQgMTAyNCIgdmVyc2lvbj0iMS4xIiB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciPjxwYXRoIGQ9Ik05NTYuMjg4IDgwNC40OEw2NDAgMjc3LjQ0VjY0aDMyYzE3LjYgMCAzMi0xNC40IDMyLTMycy0xNC40LTMyLTMyLTMyaC0zMjBjLTE3LjYgMC0zMiAxNC40LTMyIDMyczE0LjQgMzIgMzIgMzJIMzg0djIxMy40NEw2Ny43MTIgODA0LjQ4Qy00LjczNiA5MjUuMTg0IDUxLjIgMTAyNCAxOTIgMTAyNGg2NDBjMTQwLjggMCAxOTYuNzM2LTk4Ljc1MiAxMjQuMjg4LTIxOS41MnpNMjQxLjAyNCA2NDBMNDQ4IDI5NS4wNFY2NGgxMjh2MjMxLjA0TDc4Mi45NzYgNjQwSDI0MS4wMjR6IiAgLz48L3N2Zz4=)](https://github.com/Kentico/.github/blob/main/SUPPORT.md#labs-limited-support) [![CI: Build and Test](https://github.com/Kentico/repo-template/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/Kentico/repo-template/actions/workflows/ci.yml)


<!-- ABOUT THE PROJECT -->
## About The Project

Xperience by Kentico Siteimprove integration allows users to check pages for issues directly from Xperience by Kentico.

![Siteimprove CMS Plugin in Pages application Preview mode](/images/dancing-goat-siteimprove-plugin-admin.png?raw=true)

<!-- GETTING STARTED -->
## Getting Started
### Prerequisites

* Xperience by Kentico >= 26.0.0

### Installation

1. Add project reference

## Setup of CMS Plugin
By following the steps below, the integration setups automatically on the first startup.
1. In `Program.cs`, register services and map routes by adding

    ```cs
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
    
    ...
    builder.Services.AddKentico();
    builder.Services.AddSiteimprove(builder.Configuration);
    
    ...
    WebApplication app = builder.Build();
    
    ...
    app.UseKentico();
    app.UseSiteimprove();
    ```

1. In `Views/_ViewImports.cshtml`, add Tag Helper
    ```cs
    @using Kentico.Xperience.Siteimprove
    @addTagHelper *, Kentico.Xperience.Siteimprove
    ```

1. In `Views/Shared/_Layout.cshtml`, place the Tag Helper to a suitable location
    ```cs
    <page-builder-scripts />
    <siteimprove-plugin />
    ```

1. In your `appsettings.json`, add section
    ```json
    "xperience.siteimprove": {
      "APIUser": "<Siteimprove API user>",
      "APIKey": "<Siteimprove API key>",
      "EnableContentCheck" : "<true/false>" // set this to true if you are subscribed to Prepublish feature
    }
    ```

### Setup of CMS Deeplink

1. In `Program.cs`, register services by adding

    ```cs
    WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
    
    ...
    builder.Services.AddKentico();
    builder.Services.AddSiteimprove(builder.Configuration);
    ```


1. In `Views/_ViewImports.cshtml`, add Tag Helper
    ```cs
    @using Kentico.Xperience.Siteimprove
    @addTagHelper *, Kentico.Xperience.Siteimprove
    ```

1. In `Views/Shared/_Layout.cshtml`, place the Tag Helper inside the head tag
    ```cs
    <page-builder-styles />
    <siteimprove-deeplink />
    ```

<!-- USAGE EXAMPLES -->
## Usage

### CMS Plugin

The plugin is accessible only from Preview mode in the Pages application.

Pages are automatically rechecked when republished to ensure the plugin displays up-to-date information.

### CMS Deeplink

Meta tag is added to the head of each page to allow for easier setup of CMS Deeplink on Siteimprove's side. The meta tag follows pattern recommended by Siteimprove:

```html
<meta name="pageID" content="<pageID>" />
```

<!-- CONTRIBUTING -->
## Contributing

For Contributing please see [`CONTRIBUTING.md`](https://github.com/Kentico/.github/blob/main/CONTRIBUTING.md) for more information and follow the [`CODE_OF_CONDUCT`](https://github.com/Kentico/.github/blob/main/CODE_OF_CONDUCT.md)

### Requirements

* [.NET 7+ SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)

<!-- LICENSE -->
## License

Distributed under the MIT License. See [`LICENSE.md`](./LICENSE.md) for more information.

<!-- SUPPORT -->
## Support

This contribution has __Kentico Labs limited support__.

See [`SUPPORT.md`](https://github.com/Kentico/.github/blob/main/SUPPORT.md#labs-limited-support) for more information.

For any security issues see [`SECURITY.md`](https://github.com/Kentico/.github/blob/main/SECURITY.md).
