# Swallow.Components.Reactive

A custom "render mode" (-ish) for Blazor. Providing server-rendered interactivity
without having to use a long-running websocket. The goal is to be as close to
the "usual" interactive Blazor as possible, though there will nonetheless be
some parts where the gap is visible.

## In a nutshell

To enable reactive rendering, a reactive *fragment* is rendered - which simply
wraps another component; this is similar to putting `@rendermode` on a component
instance. This component is then considered *reactive*, i.e. it may react to browser
events.

In addition to the markup, all (marked) component state is persisted into the DOM
as well. When the user triggers an event - e.g. a click on a button - that event
is serialized and sent to the server, including all persisted state. On the server,
an endpoint specific to that component is invoked that first tries to rebuild the
previous state using the persisted state sent with the request. After the previous
state has been *rehydrated*, the matching event handler for the triggered event
is invoked. Finally, the resulting markup and the updated state are rendered
into a resulting DOM and sent back to the client, where the currently active
DOM is *morphed* into the target DOM. The next time an event is triggered, the
now updated state is sent alongside the event, and so on and so forth.

## Features

Reactive rendering features a custom render mode in code; you can detect whether
the reactive renderer is active by checking the `RendererInfo` of a component.
Reactive rendering is considered *interactive*, but is has a distinct name.

There can be multiple reactive fragments on a single page; they can either
contain nearly all of the DOM or just a small label. Each fragment is completely
independent of all the other fragments on the page.

### Prerendering

Every reactive fragment is prerendered by default, just like when using the
usual Blazor websocket rendering. The components' state during static rendering
is kept using the same approach as with Blazor websocket: It's persisted as a
DOM-comment, which will then be taken in by the client-side logic. The comment
will remain in the DOM; it will not be removed.

Once the client-side fragment is initialized, the component will immediately be
rerendered to properly discover the registered event handlers. During this period,
the event handlers are *not* available and the HTML elements may execute their
default behavior; a `<form>` could be submitted using a `GET` request, for example.
This could potentially leak e.g. the username and password in a login form as
query parameters, so keep this in mind.

Prerendering can be disabled for a fragment, but not globally.

### Keeping state

Every component property that is marked with `[PersistState]` will be kept in the
DOM and set for rehydration. Additionally, state may also be handled by injecting
`PersistentComponentState` to your component. Finally, you can also register
state for (scoped) services; these have to be explicitly configured for the
`IServiceCollection` using `RegisterPersistentService<TService>`, passing in
the reactive render mode.

By default, the state is JSON-serialized and compressed using Brotli, before
being protected using the `IDataProtector` and base64-encoded. A default serializer
can be registered *globally*, not per-fragment.

### Component lifecycle

In contrast to Blazor Server, the component instances are short-living for only
a single request before being destroyed again. This means that `OnInitialized`
will be invoked for every rehydration. To ensure that certain state is only
initialized once, you can keep an additional flag as state that is set to `true`
when the state has been loaded for the first time.

As the component is not rendered on the browser, `OnAfterRender` will *not* be
invoked an the `IJsRuntime` is not available.

### Event handlers

Since the lifetime of a component is short, the handler for an event needs to be
identified within the render tree. To do this, every element that has one (or
more) event handlers is considered a *trigger* and marked with a unique identifier.

This unique identifier consists of an XPath-like description of the location for
that element within the fragment, e.g. `/article/ul/li[2]/a`. If any element on
the path to the trigger has an `id`, that element is used as a new "root" for
the path, e.g. `#login-form/input[0]`.

This identification process relies on a stable rehydration; if the DOM changes
between the rendering and the rehydration using the latest state, a wrong event
handler may be invoked. Since `id`s are considered for the identification, you
can set an `id` on all elements with event handlers if the DOM is not stable.

### Streaming rendering

By default, the reactive rendering is *streamed* to the client; as soon as the
render tree is updated, the new DOM is sent to the client. This way, intermediate
rendering is possible - a button may trigger a loading process, which sets a
spinner to be displayed.

Streaming rendering can be disabled per fragment. When disabled, the DOM will
be rendered and sent to the client once after *all* event processing is done.

### Client-side processing

On the client, each reactive fragment is its own isolated "circuit" - the term
is purposefully taken from the usual interactive rendering. The circuit is
responsible for attaching the event listeners to the HTML elements as well as
sending out a request and handling the response.

If an event is triggered while a request is already running, it is added to a
queue of events. Once the first request finishes, all queued requests are sent
in a new request.

When a response is received, the DOM is swapped into the current DOM using a
morphing algorithm; this way, as much of the current DOM as possible is kept
as-is and only the changes are applied.

TODO: Client-side event modification; debounce, filters, `hx-sync`-like thingy?
TODO: Client-side dispatched events for some custom post-processing?

### Websocket transport

Instead of the default request-response model, reactive rendering can also use
a short-lived websocket. This has the benefit of enqueueing client-side events
immediately instead of having to queue them until a running request is finished.
The downside, of course, is that it means that websockets have to be configured.

This is a result of `fetch` not supporting full-duplex transport; if Browsers
start shipping this as a feature, the websockets are no longer needed.

After rehydrating the component and dispatching all events, the websocket is set
to "idle". If all events have been processed and no new events have been dispatched,
the websocket is closed automatically.

Using a websocket automatically implies streaming rendering, even when explicitly
disabled on the fragment.

TODO: Describe protocol here?

### Navigation and query parameters

Navigation can be done with the `NavigationManager` as usual. This will stop
any event processing and immediately navigate to the new page. The navigation
will *never* be done with reactive rendering, but will *always* turn into the
browser navigating to the requested page.

The current URL may be modified by setting query parameters; hash-routing is not
supported. The current query-string is attached to the request URL, which means
that every property with `[SupplyParameterFromQuery]` will be filled. When
persisting the state, all of these properties will be sent to the client as well,
where they will be used to update the current query string. This way,
`[SupplyParameterFromQuery]` can be turned into an "additional" `[PersistState]`.

### Form handling

TODO: `[SupplyParameterFromForm]` and a way to *disable* reactive handlers on
components; `<input @bind="...">` is two-way binding by default.

### `HeadContent` and `PageTitle`

The common component outlets `HeadContent` and `PageTitle` are supported in
reactive fragments. Both section outlets will be sent to the client; the page
title will simply override whatever title is currently set, while the head
content will be isolated from all other content in the `<head>`, be it from
static rendering or an other reactive fragment.

All fragments can set the page title; if multiple fragments on a single page
will render a different title, it will flip-flop between the different variants
based on which event was last processed.

### Antiforgery

Antiforgery is supported. When configured for the endpoint(s), an antiforgery
token will be rendered for the fragment and sent alongside every rendering request.

If antiforgery is not configured, no token will be set.

## Protocol

Client-Server interactions are defined by this protocol.

### Request-Response

Requests *must* contain the `srx-request` header. If an endpoint receives a
request without that header, it *must* respond with `400 Bad Request`.

Responses *must* contain the `srx-response` header. If a client receives a
response without that header, it *must* discard the response.

A valid response may have one of the following status codes:

- `200 OK`

A response containing any other status code *should* be discarded.

A `200 OK` response indicates a successful render. This response *must* have
a `multipart/mixed` content type consisting of the following blocks:

- `text/html` for the rendered markup, one or more
- `application/x-www-form-urlencoded` with an `srx-kind: state` for persisted state
- `application/x-www-form-urlencoded` with an `srx-kind: query` header for persisted query parameters
- `application/x-www-form-urlencoded` with an `srx-kind: trigger` header for trigger definitions
- `application/x-www-form-urlencoded` with an `srx-kind: antiforgery` header for antiforgery data
- `text/plain` with an `srx-redirect` header present for redirects

When a redirect is encountered, it *must* be executed immediately and the rest
of the response *must* be discarded. The connection *should* immediately be closed
and the redirect executed.

For all other blocks, they may be encountered one or more times, in which case
a later block *must* replace all content included by a previous block.

The blocks *may* be streamed to the client, i.e. arrive not at the same time. A
client *should* wait for the server to close the connection.
