# MeshCore magnetic roof mount — working context

Two standalone Onshape Feature Studios that generate a magnetic repeater mount
for a pitched steel roof. **[README.md](README.md)** is the design and assembly;
**[DFM.md](DFM.md)** is printing. This file is what you need to *change*
anything without breaking it.

| file | features |
|---|---|
| `meshcoreMagMount.fs` | `meshcoreMagMount` (base, yaw post, head, antenna cap), `meshcoreTopPlate` |
| `meshcoreRibBase.fs` | `meshcoreRibBase` — a second base for a ribbed roof pan |

The two files **share no code**. The brace's helpers were extracted verbatim
from the mount; if you fix a helper, fix both.

## Run the checkers. Always.

```
for f in meshcoreMagMount.fs meshcoreRibBase.fs; do python3 tools/audit_fs.py $f; done
python3 tools/dims.py && python3 tools/dims.py meshcoreRibBase.fs
python3 tools/preflight.py && python3 tools/brace.py
python3 tools/loads.py && python3 tools/geometry.py && python3 tools/arm.py
python3 tools/docs.py
```

| tool | what it proves |
|---|---|
| `audit_fs.py` | back-fill traps, orphan params, unused decls, never-declared names, missing helpers, namespace |
| `dims.py` | every primitive has positive extents, with the defaults actually resolved |
| `preflight.py` | geometric assertions the .fs cannot make about itself |
| `brace.py` | the rib brace, including that its premise still holds |
| `loads.py` `geometry.py` `arm.py` | magnets vs wind, clearance sweep, arm bending |
| `docs.py` | the numbers in these docs still match the .fs |

Every tool carries a copy of the defaults it needs and **asserts them against
the `.fs`**. That drift check exists because copies rotted four separate times —
`coaxW` at 8 while the .fs said 16, `PAD_Z` at 100 against 116, and so on. Each
time the checker passed by checking the wrong geometry.

**A checker that passes is only worth what it actually looked at.** Negative-test
every new guard by reintroducing the bug. Several "passing" checks turned out to
be resolving 7 symbols instead of 79, or matching zero bodies.

## Coordinate frame

Origin at the centre of the base underside, **z = 0 is the roof contact plane**,
**+X downhill**, **+Z** the roof normal at zero tilt. Pad-local **+X is
down-plane**, **+Z the panel normal**.

## Invariants — break these and it stops working

**The head's bottom is one flat plane at `azZ`.** It prints on that face with no
support. A printed test of the version with ridges hanging below it failed
outright. That is why the azimuth teeth were grooves and, now, why there are
none at all.

**Only the rib brace's magnet feet touch the roof.** Everything else is lifted
`ribClear`. That is the entire reason it can ignore rib pitch. `brace.py` checks
it from the source: every arm must start at `ribClear`, and no body except a
`pad*` may reach z=0.

**`azSteps` must divide by `azBoltN`.** Three index holes against three inserts
meant the bolts only met every 120°.

**`sleeveD` must default to 0.** `holeR = (sleeveD > 0 ? sleeveD : boltD) / 2`,
so any non-zero value bores the clutch axis to the *sleeve* whether or not one
exists. It shipped at 10.2 and caused two separate reported faults at once: the
M6 rattled, and the 10.3 A/F nut pocket was left with **1.6 mm²** of flat to
bear on where an M6 nut wants ~41.

**The clutch is single-sided.** Two rigid ears have no assembly sequence.

**MC-TOP-1 bolts come from underneath**, through ears that overhang the arm.
Behind the pad is solid arm; in front is the device. Both alternatives were
built and neither could be assembled.

**Everything on the plate's back must finish flush.** It is the mating face and
the device's own bolts sit inside the pad footprint.

**The clutch ear in `meshcoreRibBase.fs` is yawed 90°** (`RB_CLUTCH_YAW`). The
feet are narrow in X, so the ribs run along Y, and roof ribs run up-slope — Y is
the fall line, and tilting toward ±Y needs the axis along X. The cross base does
not care because you spin it on the roof; this one cannot be spun.

## Sizing decisions worth not relitigating

**Tipping governs, not sliding or strength.** Spreading magnets buys tip
resistance linearly; stacking them on thin sheet buys almost nothing, because
the sheet is already saturated.

**The azimuth needs no teeth; the clutch does.** Rotation about the mast axis is
the one direction a symmetric wind cannot load (`r × F` has no z-component when
both lie in the vertical plane through the axis), so the azimuth sees ~1.1 N·m
of gust eccentricity against 12.6 N·m of friction. The clutch takes the full
9.5 N·m overturning moment on one M6 — friction alone is SF 1.5 before creep.
**Run that comparison before removing teeth anywhere.**

**The arm was 26× overbuilt and is now 6×.** Its *depth* is not free — the coax
channel holds the top up inboard, the pad's back plane pins it outboard, and the
bottom is the print bed. The **width** was the only free dimension (32 → 24),
and then the top dropped inboard (`strutDrop` 14) once it was noticed that the
channel's height is a *choice*, not a constraint: the channel is cut down from
the top, so lowering the top lowers the channel. Use `tools/arm.py`.

**The plate is 10 mm because of what is bored from its mating face** — an 8 mm
M4 insert (8 + 2) and a flush M6 cap head (6.0 + 2.5). The cap head alone floors
it at 8.5.

**Insert bores are measured, not catalogued.** M3 4.95 → 5.0, M4 5.77 → 5.8.
The plate's had been 5.6 — squarely inside the "M4" range and still too small to
enter. Landing in the right row is not the same as fitting on the bench, and
`preflight.py` checks both separately.

**The rib brace is 11 in span, not 12.5.** 12.5 reads 143 mph but forces the
foot to reach inward from the corner instead of straddling it. Symmetry costs
9 mph. One magnet per corner would make a perfect round foot and costs 40 mph —
back level with the cross base.

## FeatureScript traps, each of which cost a real bug

1. **The bound spec's middle value is the back-fill default**, not the
   `defineFeature` map. A parameter added after an instance exists is filled
   from the bound spec. Every parameter gets **its own** spec whose middle is
   its intended default. Booleans back-fill to `false` (word them negatively);
   enums to their **first** declared value (list the default first).
2. **Changing a default never updates a placed feature.** Where that would build
   nonsense, `throw regenError(... "DELETE THIS FEATURE AND RE-ADD IT")`. There
   are three such tripwires around the interface pad.
3. **`INTERSECTION` does not preserve target identity.** SUBTRACTION and UNION
   do. `unionInto` returns `qUnion` of *all* inputs so the survivor is found
   whichever identity won.
4. **`try silent` around a void op hides everything.** `opFillet`/`opChamfer`
   return nothing. Wrap in a function returning `true`. And never put `try
   silent` inside a ternary — that is "a parameter is out of range".
5. **Fillet a ring in one operation, with per-edge fallback.** One un-filletable
   edge in an all-or-nothing set kills the other three. Select by *position*,
   not by `qParallelEdges` over the whole body.
6. **Quote every key in the defaults map.** A bare key colliding with a std name
   is a compile error.
7. **Namespace every top-level const.** `BLEND_BOUNDS` collided with
   `onshape/std/valueBounds.fs`. `MC_*` in the mount, `RB_*` in the brace.
8. **Unpacked locals often do not share the parameter's name** — `azTeeth` →
   `azN`, `clutchTeeth` → `nTeeth`. Grep the unpack before writing a guard.
   `azTeeth % azBoltN` against a non-existent local produced only "Error
   regenerating" with no line and no name.
9. **`%` — use `floor(a / b) * b` instead.** Not worth betting a paste on.
10. **A loop variable makes a block's name dynamic and its corners
    unevaluable**, which puts it beyond `dims.py`. Write repeated bodies out.
11. **Copying a helper does not bring its callees.** `ringAt` calls
    `filletEdgeSet`; extracting one without the other gave "Function
    filletEdgeSet with 4 argument(s) not found". `audit_fs.py` now flags a call
    to a name a sibling `.fs` defines and this one does not.

## Booleans: what actually causes "non-manifold"

Four consecutive failures, all different, all in this list:

* **coincident coaxial cylinders** — two cylinders sharing a radius and an
  overlapping z range. Never give two bodies the same radius on the same axis.
* **tangential line contact** — a cylinder's apex meeting a plane. The tongue is
  a disc in XZ so its apex is a *line* at `clutchZ + clutchOD/2`; when
  `clutchOD` went 50 → 56 that equalled a hard-coded flange bottom. **Derive
  heights from the features that set them**, never hard-code a coincidence.
* **zero-thickness feather edges** — the arm tapering to nothing where the pad
  plane exits its underside. `strutEnd` stops 5 mm short for this reason.
* **partial-face T-junctions** between two cut tools that meet exactly rather
  than overlapping. Overlap cut tools by a few mm.
* **tooth-cutter tangency** where cutter width exactly equals angular pitch.

`preflight.py` has `coaxial_check()` and `tangency_check()` for the first two.

## Trim bodies: say which half is being DELETED

A trim decides which half survives by **where the body sits**, not by where its
face is. The arm shoulder's ramp trim was written as "a wedge under the ramp
line" and kept the exact complement — a fin at `rampX` and nothing from
`padFull` out, the one place the shoulder exists for. Every dimension checked
out; only the side was wrong.

The rule is not about the ramp, it is about what the body *is*:

* a trim on a **solid** removes what it overlaps — put it where you want
  material *gone*;
* a trim on a **cut tool** leaves that material in the part — the coax exit ramp
  sits *below* its ramp line, the opposite of the shoulder's.

Same trap in `ftrimA/B`, `ptrimA/B`, `ktrim`, `taper*` and `faceTrim`.
`preflight.py` now models the arm's top profile and asserts it is monotonic and
covers the pad.

## Prefer adding a solid to cutting one

The arm's shoulder is unioned in, never chopped. A chop big enough to take that
band off the strut *and* the flare also reaches the mast, the N pad and the
antenna cap, and at its outboard end clips the pad's back corner and leaves
exactly the sliver that makes a boolean non-manifold.

## Warnings

**No warning may fire at the defaults.** One that does trains you to ignore
warnings. `preflight.py` mirrors every warning condition and asserts all quiet.

Mirroring is only as good as the two copies agreeing. The pad-reach test exists
**twice** in the .fs — a `regenError` and a `reportFeatureWarning` — and only
one got corrected, while `preflight.py`'s own copy was already right, so it
passed while Onshape warned. When a guard's expression changes, grep for every
copy, and *pin the expression* in `preflight.py` rather than only mirroring its
value.

That expression, for the record: the pad's furthest point in **x** is on the
mating face (`ifL/2`, lz = 0); its lowest point in **z** is the back face's
down-plane corner (`ifL/2 + ifPadT`). Mixing them up is what fired.

Watch for thresholds referenced to something that moves — dropping the arm 14 mm
dropped the coax outlet with it, and the outlet had been written against its own
constant rather than the channel floor.

## Assertions must prove existence, not just absence

Deleting the brace's feet without writing the new ones left a base with
**nothing touching the roof**, and every check passed: "no body breaks the rule"
is trivially true when there are no bodies. `brace.py` now asserts four foot
bars and eight bosses exist, one per corner.

## Open / worth knowing

* **P1-Pro mass is an estimate** (1.3 kg). Every wind number scales with it.
* **The enclosure's rib height is unmeasured**, hence `stripProud = 0`.
* **The rib pitch is unknown** and deliberately does not matter — see the brace.
* The user reviews in Onshape and reports back with screenshots. **Take those
  reports literally.** Every single time the model was right and the first
  explanation offered was wrong.
* Deleting and re-adding the feature is the user's normal workflow, so a
  stale-instance theory is almost never the explanation.
