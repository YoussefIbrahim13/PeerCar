/* =====================================================================
   PeerCar — interaction layer
   Small, dependency-free enhancements. Every feature degrades gracefully:
   without JS the pages still render and work.
   ===================================================================== */
(function () {
    "use strict";

    const root = document.documentElement;
    const reduceMotion = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
    const $ = (s, ctx = document) => ctx.querySelector(s);
    const $$ = (s, ctx = document) => Array.from(ctx.querySelectorAll(s));
    const store = {
        get(k) { try { return localStorage.getItem(k); } catch { return null; } },
        set(k, v) { try { localStorage.setItem(k, v); } catch { /* storage unavailable */ } }
    };

    /* ---------- Theme ---------- */
    function applyTheme(theme) {
        root.setAttribute("data-bs-theme", theme);
        const meta = $('meta[name="theme-color"]');
        if (meta) meta.setAttribute("content", theme === "dark" ? "#0a0b08" : "#f3f1ea");
    }
    function initTheme() {
        $$("[data-theme-toggle]").forEach(btn => btn.addEventListener("click", () => {
            const next = root.getAttribute("data-bs-theme") === "dark" ? "light" : "dark";
            root.classList.add("pc-theme-anim");
            applyTheme(next);
            store.set("pc-theme", next);
            setTimeout(() => root.classList.remove("pc-theme-anim"), 500);
        }));
    }

    /* ---------- Navbar ---------- */
    function initNavbar() {
        const nav = $(".pc-navbar");
        if (!nav) return;
        let menuOpen = false;
        // While the mobile menu is open, use the solid bar so the logo stays readable over any background.
        const onScroll = () => nav.classList.toggle("is-scrolled", menuOpen || window.scrollY > 8);
        onScroll();
        window.addEventListener("scroll", onScroll, { passive: true });
        const collapse = $(".navbar-collapse", nav);
        if (collapse) {
            collapse.addEventListener("show.bs.collapse", () => { menuOpen = true; onScroll(); });
            collapse.addEventListener("hidden.bs.collapse", () => { menuOpen = false; onScroll(); });
        }

        // Highlight the link for the current section
        const path = location.pathname.toLowerCase().replace(/\/+$/, "") || "/";
        let best = null, bestLen = -1;
        $$(".pc-navbar .nav-link[href]").forEach(a => {
            const href = (a.getAttribute("href") || "").toLowerCase().replace(/\/+$/, "") || "/";
            if (href === "#" ) return;
            const match = href === "/" ? (path === "/" || path === "/home" || path === "/home/index") : (path === href || path.startsWith(href + "/"));
            if (match && href.length > bestLen) { best = a; bestLen = href.length; }
        });
        if (best) { best.classList.add("active"); best.setAttribute("aria-current", "page"); }
    }

    /* ---------- Scroll reveal ---------- */
    function initReveal() {
        // Give legacy pages some motion too: top-level cards in <main> reveal on scroll.
        $$(".pc-main .card").forEach(card => {
            if (!card.parentElement.closest(".card, [data-reveal], .modal, .dropdown-menu") && !card.hasAttribute("data-reveal")) {
                card.setAttribute("data-reveal", "");
            }
        });
        $$("[data-reveal-stagger]").forEach(group => {
            const step = parseFloat(group.getAttribute("data-reveal-stagger")) || 0.08;
            $$(":scope > *", group).forEach((child, i) => {
                if (!child.hasAttribute("data-reveal")) child.setAttribute("data-reveal", "");
                child.style.setProperty("--reveal-delay", (Math.min(i, 8) * step) + "s");
            });
        });

        const items = $$("[data-reveal]");
        if (reduceMotion || !("IntersectionObserver" in window)) {
            items.forEach(el => el.classList.add("is-visible"));
            return;
        }
        const io = new IntersectionObserver(entries => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add("is-visible");
                    io.unobserve(entry.target);
                }
            });
        }, { rootMargin: "0px 0px -8% 0px", threshold: 0.08 });
        items.forEach(el => io.observe(el));
    }

    /* ---------- Count-up numbers ---------- */
    function initCounters() {
        const els = $$("[data-count]");
        if (!els.length) return;
        const run = el => {
            const target = parseFloat(el.getAttribute("data-count")) || 0;
            const suffix = el.getAttribute("data-suffix") || "";
            const decimals = (String(target).split(".")[1] || "").length;
            if (reduceMotion) { el.textContent = target.toLocaleString(undefined, { maximumFractionDigits: decimals }) + suffix; return; }
            const duration = 1400, start = performance.now();
            const tick = now => {
                const p = Math.min(1, (now - start) / duration);
                const eased = 1 - Math.pow(1 - p, 4);
                el.textContent = (target * eased).toLocaleString(undefined, { minimumFractionDigits: decimals, maximumFractionDigits: decimals }) + suffix;
                if (p < 1) requestAnimationFrame(tick);
            };
            requestAnimationFrame(tick);
        };
        if (!("IntersectionObserver" in window)) { els.forEach(run); return; }
        const io = new IntersectionObserver(entries => entries.forEach(e => {
            if (e.isIntersecting) { run(e.target); io.unobserve(e.target); }
        }), { threshold: 0.4 });
        els.forEach(el => io.observe(el));
    }

    /* ---------- Button ripple ---------- */
    function initRipple() {
        document.addEventListener("pointerdown", e => {
            const btn = e.target.closest(".btn");
            if (!btn || reduceMotion || btn.classList.contains("btn-link")) return;
            const rect = btn.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            const r = document.createElement("span");
            r.className = "pc-ripple";
            r.style.width = r.style.height = size + "px";
            r.style.left = (e.clientX - rect.left - size / 2) + "px";
            r.style.top = (e.clientY - rect.top - size / 2) + "px";
            btn.appendChild(r);
            setTimeout(() => r.remove(), 650);
        });
    }

    /* ---------- Cursor spotlight + tilt ---------- */
    function initPointerFx() {
        if (reduceMotion || !window.matchMedia("(hover: hover)").matches) return;
        document.addEventListener("pointermove", e => {
            const el = e.target.closest(".pc-spotlight");
            if (!el) return;
            const r = el.getBoundingClientRect();
            el.style.setProperty("--mx", (e.clientX - r.left) + "px");
            el.style.setProperty("--my", (e.clientY - r.top) + "px");
        }, { passive: true });

        $$("[data-tilt]").forEach(el => {
            const max = parseFloat(el.getAttribute("data-tilt")) || 8;
            const lift = el.hasAttribute("data-tilt-lift") ? " translateY(-8px)" : "";
            let raf = 0;
            el.addEventListener("pointermove", e => {
                cancelAnimationFrame(raf);
                raf = requestAnimationFrame(() => {
                    const r = el.getBoundingClientRect();
                    const x = (e.clientX - r.left) / r.width - 0.5;
                    const y = (e.clientY - r.top) / r.height - 0.5;
                    el.style.transition = "transform .15s ease-out, box-shadow .5s, border-color .3s";
                    el.style.transform = `perspective(1000px) rotateX(${(-y * max).toFixed(2)}deg) rotateY(${(x * max).toFixed(2)}deg)${lift}`;
                });
            });
            el.addEventListener("pointerleave", () => { cancelAnimationFrame(raf); el.style.transition = ""; el.style.transform = ""; });
        });

        // Magnetic buttons: drift toward the cursor, spring back on leave
        $$("[data-magnetic]").forEach(el => {
            const strength = parseFloat(el.getAttribute("data-magnetic")) || 0.35;
            el.addEventListener("pointermove", e => {
                const r = el.getBoundingClientRect();
                const x = (e.clientX - r.left - r.width / 2) * strength;
                const y = (e.clientY - r.top - r.height / 2) * strength;
                el.style.translate = `${x.toFixed(1)}px ${y.toFixed(1)}px`;
            });
            el.addEventListener("pointerleave", () => { el.style.translate = ""; });
        });

        // Parallax: layers inside the landing hero follow the pointer at different depths
        const hero = $(".pc-hero");
        if (hero) {
            const layers = $$("[data-parallax]", hero);
            const blobs = $$(".pc-blob", hero);
            hero.addEventListener("pointermove", e => {
                const r = hero.getBoundingClientRect();
                const x = (e.clientX - r.left) / r.width - 0.5;
                const y = (e.clientY - r.top) / r.height - 0.5;
                layers.forEach(l => {
                    const d = parseFloat(l.getAttribute("data-parallax")) || 10;
                    l.style.translate = `${(-x * d).toFixed(1)}px ${(-y * d).toFixed(1)}px`;
                });
                blobs.forEach((b, i) => { b.style.translate = `${(x * (30 + i * 20)).toFixed(1)}px ${(y * (30 + i * 20)).toFixed(1)}px`; });
            });
            hero.addEventListener("pointerleave", () => layers.forEach(l => { l.style.translate = ""; }));
        }
    }

    /* ---------- Split headline into animated words ---------- */
    function initSplit() {
        if (reduceMotion) return;
        $$("[data-split]").forEach(el => {
            let i = 0;
            const wrap = node => {
                const w = document.createElement("span");
                w.className = "pc-w";
                const inner = document.createElement("span");
                inner.style.setProperty("--i", String(i++));
                inner.appendChild(node);
                w.appendChild(inner);
                return w;
            };
            Array.from(el.childNodes).forEach(node => {
                if (node.nodeType === Node.TEXT_NODE) {
                    const frag = document.createDocumentFragment();
                    node.textContent.split(/(\s+)/).forEach(part => {
                        if (!part) return;
                        frag.appendChild(/^\s+$/.test(part) ? document.createTextNode(part) : wrap(document.createTextNode(part)));
                    });
                    el.replaceChild(frag, node);
                } else if (node.nodeType === Node.ELEMENT_NODE) {
                    el.replaceChild(wrap(node.cloneNode(true)), node);
                }
            });
        });
    }

    /* ---------- Reading progress ---------- */
    function initScrollProgress() {
        const bar = $(".pc-scroll-progress");
        if (!bar) return;
        let ticking = false;
        const update = () => {
            const max = document.documentElement.scrollHeight - innerHeight;
            bar.style.transform = `scaleX(${max > 0 ? Math.min(1, scrollY / max) : 0})`;
            ticking = false;
        };
        update();
        window.addEventListener("scroll", () => { if (!ticking) { ticking = true; requestAnimationFrame(update); } }, { passive: true });
    }

    /* ---------- Back to top (with scroll progress ring) ---------- */
    function initToTop() {
        const btn = $(".pc-to-top");
        if (!btn) return;
        const circle = $("circle", btn);
        const onScroll = () => {
            const max = document.documentElement.scrollHeight - innerHeight;
            const p = max > 0 ? scrollY / max : 0;
            btn.classList.toggle("is-visible", scrollY > 500);
            if (circle) circle.style.setProperty("--pc-scroll-dash", String(150 - 150 * p));
        };
        onScroll();
        window.addEventListener("scroll", onScroll, { passive: true });
        btn.addEventListener("click", () => window.scrollTo({ top: 0, behavior: reduceMotion ? "auto" : "smooth" }));
    }

    /* ---------- Toasts ---------- */
    function dismissToast(t) {
        if (!t || t.classList.contains("is-leaving")) return;
        t.classList.add("is-leaving");
        setTimeout(() => t.remove(), 360);
    }
    function initToasts() {
        $$(".pc-toast").forEach(t => {
            let timer = setTimeout(() => dismissToast(t), 6000);
            t.addEventListener("mouseenter", () => clearTimeout(timer));
            t.addEventListener("mouseleave", () => { timer = setTimeout(() => dismissToast(t), 2500); });
            const close = $(".pc-toast-close", t);
            if (close) close.addEventListener("click", () => dismissToast(t));
        });
    }
    // Public helper so page scripts can raise a toast: PeerCar.toast("Saved!", "success")
    function toast(message, type = "info") {
        let host = $(".pc-toasts");
        if (!host) { host = document.createElement("div"); host.className = "pc-toasts"; document.body.appendChild(host); }
        const icons = { success: "bi-check2-circle", error: "bi-exclamation-octagon", info: "bi-info-circle" };
        const t = document.createElement("div");
        t.className = `pc-toast is-${type}`;
        t.setAttribute("role", type === "error" ? "alert" : "status");
        t.innerHTML = `<div class="pc-toast-icon"><i class="bi ${icons[type] || icons.info}"></i></div><div class="pc-toast-body"></div><button type="button" class="pc-toast-close" aria-label="Close"><i class="bi bi-x-lg"></i></button><span class="pc-toast-timer"></span>`;
        $(".pc-toast-body", t).textContent = message;
        host.appendChild(t);
        $(".pc-toast-close", t).addEventListener("click", () => dismissToast(t));
        setTimeout(() => dismissToast(t), 6000);
    }

    /* ---------- Smooth page transitions ---------- */
    function initTransitions() {
        if (reduceMotion) return;
        document.addEventListener("click", e => {
            const a = e.target.closest("a[href]");
            if (!a || e.defaultPrevented || e.button !== 0 || e.metaKey || e.ctrlKey || e.shiftKey || e.altKey) return;
            const href = a.getAttribute("href");
            if (!href || href.startsWith("#") || href.startsWith("javascript:") || href.startsWith("mailto:") || href.startsWith("tel:")) return;
            if (a.target && a.target !== "_self") return;
            if (a.hasAttribute("download") || a.hasAttribute("data-bs-toggle") || a.hasAttribute("data-no-transition")) return;
            const url = new URL(a.href, location.href);
            if (url.origin !== location.origin) return;
            if (url.pathname === location.pathname && url.search === location.search && url.hash) return;
            document.body.classList.add("pc-leaving");
        });
        // Restore when navigating back (bfcache) or when a download/new tab keeps us here
        window.addEventListener("pageshow", () => document.body.classList.remove("pc-leaving"));
        window.addEventListener("blur", () => setTimeout(() => document.body.classList.remove("pc-leaving"), 1200));
    }

    /* ---------- Forms: loading state, password toggles, strength ---------- */
    function initForms() {
        document.addEventListener("submit", e => {
            const form = e.target;
            const btn = e.submitter && e.submitter.matches(".btn") ? e.submitter : $('button[type="submit"].btn, .btn[type="submit"]', form);
            if (!btn || form.hasAttribute("data-no-loading")) return;
            // Let other handlers (confirm(), jQuery validation) decide first.
            setTimeout(() => {
                if (e.defaultPrevented) return;
                if (window.jQuery && jQuery(form).data("validator") && !jQuery(form).valid()) return;
                btn.style.width = btn.offsetWidth + "px";
                btn.classList.add("is-loading");
                document.body.classList.add("pc-leaving");
            }, 0);
        });
        window.addEventListener("pageshow", () => $$(".btn.is-loading").forEach(b => { b.classList.remove("is-loading"); b.style.width = ""; }));

        $$("[data-toggle-password]").forEach(btn => {
            btn.addEventListener("click", () => {
                const input = $(btn.getAttribute("data-toggle-password"));
                if (!input) return;
                const show = input.type === "password";
                input.type = show ? "text" : "password";
                const icon = $("i", btn);
                if (icon) { icon.classList.toggle("bi-eye", !show); icon.classList.toggle("bi-eye-slash", show); }
                btn.setAttribute("aria-label", show ? "Hide password" : "Show password");
            });
        });

        $$("[data-strength-for]").forEach(meter => {
            const input = $(meter.getAttribute("data-strength-for"));
            if (!input) return;
            const label = meter.nextElementSibling && meter.nextElementSibling.matches("[data-strength-label]") ? meter.nextElementSibling : null;
            const words = ["", "Weak", "Fair", "Good", "Strong"];
            input.addEventListener("input", () => {
                const v = input.value;
                let s = 0;
                if (v.length >= 6) s++;
                if (v.length >= 10) s++;
                if (/[A-Z]/.test(v) && /[a-z]/.test(v)) s++;
                if (/\d/.test(v) && /[^A-Za-z0-9]/.test(v)) s++;
                if (v.length && s === 0) s = 1;
                meter.setAttribute("data-score", String(s));
                if (label) label.textContent = v ? words[s] : "";
            });
        });

        // Live preview for image file inputs: <input type="file" data-preview="#img">
        $$('input[type="file"][data-preview]').forEach(input => {
            input.addEventListener("change", () => {
                const img = $(input.getAttribute("data-preview"));
                const file = input.files && input.files[0];
                if (!img || !file || !file.type.startsWith("image/")) return;
                img.src = URL.createObjectURL(file);
                img.classList.remove("d-none");
            });
        });
    }

    /* ---------- Lightbox ---------- */
    function initLightbox() {
        const triggers = $$("[data-lightbox]");
        if (!triggers.length) return;
        const box = document.createElement("div");
        box.className = "pc-lightbox";
        box.setAttribute("role", "dialog");
        box.setAttribute("aria-modal", "true");
        box.innerHTML = "<img alt=\"\" />";
        document.body.appendChild(box);
        const img = $("img", box);
        const close = () => box.classList.remove("is-open");
        triggers.forEach(t => t.addEventListener("click", () => {
            const src = t.getAttribute("data-lightbox") || ($("img", t) || {}).src;
            if (!src) return;
            img.src = src;
            img.alt = t.getAttribute("aria-label") || "";
            box.classList.add("is-open");
        }));
        box.addEventListener("click", close);
        document.addEventListener("keydown", e => { if (e.key === "Escape") close(); });
    }

    /* ---------- Saved cars (per-browser favourites) ---------- */
    function initFavourites() {
        const key = "pc-favs";
        let favs;
        try { favs = new Set(JSON.parse(store.get(key) || "[]")); } catch { favs = new Set(); }
        $$("[data-fav]").forEach(btn => {
            const id = btn.getAttribute("data-fav");
            const sync = () => {
                const on = favs.has(id);
                btn.classList.toggle("is-active", on);
                btn.setAttribute("aria-pressed", String(on));
                btn.setAttribute("aria-label", on ? "Remove from saved" : "Save car");
            };
            sync();
            btn.addEventListener("click", e => {
                e.preventDefault(); e.stopPropagation();
                favs.has(id) ? favs.delete(id) : favs.add(id);
                store.set(key, JSON.stringify([...favs]));
                sync();
                if (!reduceMotion) btn.animate([{ transform: "scale(1)" }, { transform: "scale(1.35)" }, { transform: "scale(1)" }], { duration: 420, easing: "cubic-bezier(.34,1.56,.64,1)" });
            });
        });
    }

    /* ---------- Images with fallbacks ---------- */
    function initImages() {
        $$("img[data-fallback]").forEach(img => {
            const swap = () => {
                const fb = img.getAttribute("data-fallback");
                img.removeAttribute("data-fallback");
                if (fb === "hide") { img.style.display = "none"; return; }
                img.src = fb;
            };
            if (img.complete && img.naturalWidth === 0 && img.getAttribute("src")) swap();
            else img.addEventListener("error", swap, { once: true });
        });
    }

    /* ---------- Bootstrap tooltips ---------- */
    function initTooltips() {
        if (!window.bootstrap || !bootstrap.Tooltip) return;
        $$('[data-bs-toggle="tooltip"]').forEach(el => new bootstrap.Tooltip(el));
    }

    function boot() {
        initTheme();
        initNavbar();
        initReveal();
        initCounters();
        initRipple();
        initSplit();
        initPointerFx();
        initScrollProgress();
        initToTop();
        initToasts();
        initTransitions();
        initForms();
        initLightbox();
        initFavourites();
        initImages();
        initTooltips();
    }

    window.PeerCar = { toast, applyTheme };
    if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", boot);
    else boot();
})();
