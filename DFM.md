# Printing — FDM, ASA

ASA or ASA-CF/GF. PLA will sag on a sunny roof. Design rationale is in
[CLAUDE.md](CLAUDE.md); this is what you need at the slicer.

## Orientation, part by part

| part | orientation | height | ~mass | support |
|---|---|---|---|---|
| Base (cross) | flat, underside on the bed | 73 mm | ~215 g | none |
| Base (round) | flat, underside on the bed | 12 mm | ~95 g | none |
| Yaw post | **azimuth flange on the bed, upside down** | 62 mm | ~41 g | light, under the tongue disc |
| Head | **flat on its bottom face** | 60 mm | ~100 g | none |
| Antenna cap | flat | 9 mm | ~6 g | none |
| Top plate | back face down | 10 mm | ~45 g | none |
| Rib brace | flat, feet on the bed | 30 mm | ~170 g | **yes — under the arms** |

Masses are estimates at 4 walls / 20 % infill. Mount total ≈ 400 g.

## The head prints flat, and that is load-bearing

Its **entire bottom is one plane at `azZ`**. A printed test of an earlier version
with azimuth ridges hanging below that plane failed outright — the part came off
the bed. Two consequences that look like free choices and are not:

* the azimuth joint has **no teeth at all** now. Both faces are plain flat
  annuli, which is the best first layer either the head or the post can get;
* the arm's underside sits on that same plane, so the arm is a deep wedge rather
  than a beam floating above it — which also took the root second moment from
  46,900 to 146,300 mm⁴, and is why there is no knee brace.

Anything you add below `azZ` puts the whole part back on supports.
`preflight.py` fails if a body reappears there.

## The yaw post has no good orientation, and that is inherent

It carries two mating faces 90° apart: the clutch teeth face along Y, the
azimuth index plate faces along Z. No orientation prints both well.

**Print it azimuth-flange-down.** That gives a Ø72 flat disc as the first layer —
the best adhesion of any part here — and puts the clutch teeth on a *vertical
wall*, which is exactly how the base's ear teeth print, so the two mating faces
share their error rather than fighting it.

The tongue disc needs a little support underneath. It is small and on a
non-mating face.

## The rib brace needs support, and it is the only part that does

Its arms' undersides sit `ribClear` (6.35 mm) up and are anchored **only at the
feet**, so there is nothing for them to grow from. This is not an oversight —
every alternative is worse:

* a 45° self-supporting underside makes a 157 mm arm 157 mm tall;
* sloping it gently enough to stay short gives an 88° overhang;
* printing it upside down puts the clutch ear into the bed;
* splitting the hub off as a second bolted part avoids support entirely, and is
  the fallback if you would rather not.

The support is about as benign as it gets: flat ceilings 6.35 mm off the bed, on
a non-functional face pointing at the roof. Roughly 25–30 cm³.

## Clutch teeth are near the resolution floor

36 teeth on a Ø42 ring is **1.98 mm deep** with a ~3.7 mm pitch at the OD. That
is only about 9 extrusion widths per flank at 0.4 mm.

* print the clutch pair at **0.15 mm layers**, or at least the post;
* the flanks carry the load, not the tips — there is root clearance under them,
  so a tip flattened by 0.2 mm just seats 0.2 mm shallower;
* **print the clutch pair first as a standalone fit test.** Tooth engagement and
  the M6 fit are the only things that can quietly be wrong.

`preflight.py` fails below 0.5 mm of tooth depth, and warns if the cutter width
only just reaches the pitch at the OD — adjacent grooves that touch rather than
overlap are a non-manifold surface, not a cosmetic issue.

## The base is the classic ASA warp case

A 260 mm flat plate with thin extremities. The rib brace is worse at 336 mm.

* enclosure, or at minimum no draught;
* 100–110 °C bed, brim on the arm tips;
* the ground edge is already broken by `edgeBreak`, which relieves elephant foot
  where it would otherwise lift the magnet pockets off the roof.

## Smaller things

* **Magnet pockets are open at the bottom on purpose.** A 0.4 mm print membrane
  under a magnet costs about a tenth of the pull and a saturating roof sheet has
  none to spare. There is deliberately no push-out hole either — a hole in an
  upward-facing surface drains onto a bare magnet and pools there.
* **Heat-set inserts, never self-tappers.** Bores are sized to *measured*
  inserts: M3 at 4.95 mm plain OD → Ø5.0, M4 at 5.77 → Ø5.8. Measure yours.
* **Nothing is countersunk.** Cap heads throughout — a 90° cone printed into a
  45° face was never much of a seat.
* **The coax channel exit ramps** up to the arm's surface over its last 16 mm, so
  the cable walks out without pliers.
* The interface pad is **tapered in plan**, narrow at its down-plane end. That is
  a print constraint, not styling: the ears overhang the arm, so a rectangular
  pad's first layer would be a 16 mm line laid into open air on each side.

## Print order

1. **Clutch pair** — the post and whichever base. Fit test before anything else.
2. Antenna cap, then the head. Check the bulkhead and the pigtail on the bench.
3. Base.
4. Top plate last — it is the part most likely to change when you measure the
   enclosure's rib.
