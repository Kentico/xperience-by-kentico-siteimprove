# Xperience by Kentico Siteimprove

<!-- ABOUT THE PROJECT -->
## About The Project

Xperience by Kentico Siteimprove integration allows users to check pages for issues directly from Xperience by Kentico.

![Siteimprove CMS Plugin in Pages application Preview mode](/images/dancing-goat-siteimprove-plugin-admin.png?raw=true)

<!-- GETTING STARTED -->
## Getting Started
### Prerequisites

* Xperience by Kentico >= 26.5.0

### Installation

1. Add project reference or NuGet from provided file (NuGet only for Xperience by Kentico 26.5.0)

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
