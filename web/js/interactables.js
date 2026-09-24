export function createInteractableBus() {
  const registry = new Map();
  const listeners = new Set();

  function emit(id, action, extra) {
    const entry = registry.get(id) ?? { id, kind: "unknown", label: id };
    const event = { id, action, kind: entry.kind, label: entry.label, extra };
    for (const fn of listeners) fn(event);
    if (action === "use" && typeof entry.onUse === "function") entry.onUse(event);
    if (action === "grab" && typeof entry.onGrab === "function") entry.onGrab(event);
    if (action === "hover" && typeof entry.onHover === "function") entry.onHover(event);
    return event;
  }

  return {
    register(def) {
      registry.set(def.id, def);
      return () => registry.delete(def.id);
    },
    list() {
      return [...registry.values()].map(({ id, kind, label }) => ({ id, kind, label }));
    },
    use(id) {
      return emit(id, "use");
    },
    grab(id) {
      return emit(id, "grab");
    },
    hover(id, on = true) {
      return emit(id, "hover", { on });
    },
    on(fn) {
      listeners.add(fn);
      return () => listeners.delete(fn);
    },
  };
}
