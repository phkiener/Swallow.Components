# Open tasks

## Reactive

- Imitate `hx-sync` (queue-all especially)
- Add loading indicator/interaction stopper?
- Support debouncing
- Try sending out DOM-diffs; RenderBatch full of RenderFrames?
- `hx-preserve` needed?
- Can we do URLs based on `{type}/{trigger}/{event}`?
  - This makes more sense with identifiers, tho
- Handle `@key` in identifiers, too?

## Components

- Think about which components to include
- Think long and hard about CSS classes vs. component styles
- Design System; add framework-y components
  - "Output and code"-view, possibly with "pure DOM" too
  - Marker "safe for static rendering"?
- Icons: Embed or reference, toggleable (globally or individually?)

## Utilities

- RouteGenerator: Handle route parameters -> methods instead of properties
