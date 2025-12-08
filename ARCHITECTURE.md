# Architectural decisions

Because proper ADRs feel like way to much text and I just need a single place
where I put all my considerations and thoughts.

## Decisions

### Components support all render modes

Every component should work in every rendermode (static, websocket, WASM). For
the interactive rendermodes, this should probably not be an issue - some
DOM-events will have JS-based listeners to not pay the latency of the websocket
when handling the event. Static rendering is a different beast though; there
is no rendering after the first pass. So every interaction *has* to be defined
in JS, even when an e.g. `@onclick` would be more fitting.

To accomodate, some components will need to detect the current rendermode and
render differently based on that; ready for JS-handlers for static rendering
and with "plain" Blazor handlers for interactive rendering.

Good thing is: We can just add the event handlers to elements even when
rendering statically; they'll simply not be rendered to the DOM. Which is good
in our case!

### Components will not use isolated styles (`*.razor.css`)

Isolated styles are nice, but they do impose some restrictions - reusing styles
is *hard* because of the isolation. Layering will probably not work well,
nested CSS has issues...

All in all, using only isolated styles would be limiting the freedom we have.
Due to that, all component styles will be defined in plain stylesheets. These
stylesheets will be layered:

- layer `reset` to remove unwantend user-agent styles
- layer `components` for all component styles
- possibly some additional layers?

Component styles will be denoted by classes with the `sw-` prefix; `sw-button`,
`sw-input`, `sw-whatever`. Components will use these classes to retrieve the
styles, which means user-defined components can opt-in to these styles as well.

Due to this, a user can define their fully custom styling as well. This makes
the component library be a combination of two different concerns: A library
of primitives handling the DOM and interactions, as well as a css "framework"
of sorts to style these primitives. Each of these *can* live without the other,
but that's not the primary usecase.

### Static Interactive rendering as part of this library

Static rendering is awesome, but the fact that interactivity is limited to
`<form>` is more than annoying. Interactive rendering is convenient, but having
a persistent websocket connection or multiple megabytes of WASM as a
requirement hinders the usability. Yes, it's possible... but it falls behind
other web technologies.

In other parts, I have experimented with using [HTMX](https://htmx.org) to add
a layer of interactivity on top of static rendering - which works okay-ish.
There's a handful of drawbacks, most notably having to explicitly set an
identifier for each element with an event handler, which make it difficult to
use. I want to experiment more on this topic.

The current version works as follows:

1. The component is rendered statically, if prerendering is active
2. A clientside event triggers another render using a *different* `HtmlRenderer`
    * This render puts all persisted state into the DOM as hidden inputs
3. HTMX attaches listeners to every eventhandler
4. When an event is triggered, a request is sent to rerender the component
   * All persisted state is sent as part of that request
   * Information about the event is sent as well
5. The matching event handler is invoked and the component is redrawn
6. The response includes the updated DOM plus all updated state
7. HTMX then replaces the DOM (using morphdom in this case) and the state
8. The next event would trigger a new request containing the updated state

- can the internal `eventHandlerId` be exposed and used - is it stable?
- can we work around having to send *all* state in *every* request?
- can we trigger more than one render after an event, by streaming maybe?

This rendering will be included as part of the library, though as an opt-in
package alongside it. A user may use the new rendermode, but it must be fully
optional. All components should *also* respect this rendermode and work
out of the box, without any additions. If I can't work around having a
unique identifier on the elements, so be it - though that's the most pressing
issue right now.

### General utility as part of this library

Blazor feels nice to use, but some things are weirdly missing. This library
will contain a selection of useful... stuffs. The collection will grow while
building the library and demo page, including but not limited to:

- generating URLs from components, i.e. `NavigationManager.NavigateTo<Page>()`
