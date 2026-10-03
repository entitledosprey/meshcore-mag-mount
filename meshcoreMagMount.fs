FeatureScript 3083;
import(path : "onshape/std/common.fs", version : "3083.0");

// =====================================================================
//  MeshCore magnetic roof mount
//
//  A repeater mount for a pitched standing-seam steel roof. Sticks down
//  with neodymium discs, brings the antenna mast back to plumb whatever
//  the roof pitch is, carries an N-type antenna, and takes different
//  repeater hardware by swapping one top plate.
//
//  TWO JOINTS, NOT THREE. Aiming this thing needs three rotations, but
//  only two need hardware:
//
//    1. Line the tilt plane up with the roof's fall line - free. The
//       base is magnetic, so you just set it down rotated the way you
//       want it. No mechanism.
//    2. Plumb the mast - serrated face clutch at the base, in double
//       shear, one M6 through the axis.
//    3. Aim the panel - toothed ring ABOVE the clutch, turning about the
//       now-plumb mast axis. It has to be above the clutch: at the base
//       it would turn about the roof normal instead of about vertical,
//       and panel azimuth would stay coupled to mast tilt.
//
//  TIPPING GOVERNS, NOT STRENGTH. Four D82B discs are rated 32 lbf but
//  deliver about 13 on a thin painted roof panel, because a 24 ga sheet
//  saturates long before the magnet does. That is also why stacking
//  magnets is close to worthless here and spreading them is not -
//  spreading multiplies force AND lever. Eight discs as tangential pairs
//  at r=120 takes it from 61 mph to 104. See tools/loads.py.
//
//  NOTHING CROSSES A MOVING JOINT. The N bulkhead and the device arm are
//  both on the head, so the coax pigtail never crosses the azimuth ring
//  or the tilt clutch. It runs down the mast bore and out to the arm.
//
//  PARTS
//    Base      - hub, arms, magnet and yoke pockets, clutch ears, tether eye
//    Yaw post  - clutch tongue, column, azimuth tooth face, captive nyloc
//    Head      - azimuth tooth face, mast, N bulkhead pad, 45 deg arm,
//                MC-TOP-1 interface
//    Top plate - separate feature, one per device
//
//  TURNED PARTS (6061 / steel bar, the lathe earns its keep here)
//    8 x steel yoke disc, d12.7 x 3 - flux return behind each magnet,
//        worth about +30% pull. The single highest-value metal part.
//    1 x alu clutch sleeve, d10 OD x d6.2 ID x tongue width - keeps the
//        M6 from ovalising the plastic bore.
//    1 x alu N backing washer, d20 x 2 - stops the bulkhead nut creeping
//        into hot plastic. Deliberately SMALL: these are end-fed 915 MHz
//        antennas and want no ground plane under the feed.
//
//  PRINTING - ASA or ASA-CF. PLA will sag on a roof.
//    Base      : flat, right way up. Magnet pockets open downward, so
//                they need no support and the magnets sit dead flush.
//    Yaw post  : tongue face down.
//    Head      : azimuth face down on the bed.
//    Top plate : flat.
//
//  SELF-CONTAINED. The two declaration lines are already at the top, so
//  select all in the Feature Studio, delete, and paste this whole file.
//  If Onshape is on a newer release than 3083, bump BOTH numbers to match
//  what a fresh Feature Studio shows you - they have to agree.
// =====================================================================

// ---------------------------------------------------------------- enums
export enum MountParts
{
    annotation { "Name" : "All three" }
    ALL,
    annotation { "Name" : "Base only" }
    BASE,
    annotation { "Name" : "Yaw post only" }
    YAW_POST,
    annotation { "Name" : "Head only" }
    HEAD
}

export enum BaseStyle
{
    annotation { "Name" : "Cross arms - thin sheet, spread wide" }
    CROSS,
    annotation { "Name" : "Round disc - thick plate, compact" }
    ROUND
}

export enum NutStyle
{
    annotation { "Name" : "Captive nyloc in a hex pocket" }
    CAPTIVE,
    annotation { "Name" : "Plain through hole, nut outside" }
    THROUGH
}

// --------------------------------------------------------------- bounds
// EVERY parameter gets its own bound spec, and the MIDDLE value is the
// intended default. Onshape back-fills a newly added parameter on an
// existing feature instance from the BOUND SPEC, not from the defaults
// map - sharing one spec between parameters that want different defaults
// silently rebuilds old instances in the wrong place.
const MC_ROUNDD_BOUNDS = { (millimeter) : [70.0, 152.4, 320.0] } as LengthBoundSpec;
const MC_ROUNDT_BOUNDS = { (millimeter) : [8.0, 12.0, 30.0] } as LengthBoundSpec;
const MC_ROUNDMAGR_BOUNDS = { (millimeter) : [20.0, 60.0, 150.0] } as LengthBoundSpec;
const MC_ROUNDMAGS_BOUNDS = { (unitless) : [3, 8, 24] } as IntegerBoundSpec;
const MC_MAGR_BOUNDS = { (millimeter) : [50.0, 120.0, 170.0] } as LengthBoundSpec;
const MC_MAGPITCH_BOUNDS = { (millimeter) : [14.0, 32.0, 60.0] } as LengthBoundSpec;
const MC_MAGD_BOUNDS = { (millimeter) : [5.0, 12.9, 40.0] } as LengthBoundSpec;
const MC_MAGT_BOUNDS = { (millimeter) : [1.0, 3.175, 12.0] } as LengthBoundSpec;
const MC_YOKET_BOUNDS = { (millimeter) : [0.0, 3.0, 12.0] } as LengthBoundSpec;
const MC_MAGWALL_BOUNDS = { (millimeter) : [1.2, 3.0, 8.0] } as LengthBoundSpec;
const MC_PUSHD_BOUNDS = { (millimeter) : [0.0, 0.0, 10.0] } as LengthBoundSpec;
const MC_ARMW_BOUNDS = { (millimeter) : [10.0, 24.0, 60.0] } as LengthBoundSpec;
const MC_ARMROOT_BOUNDS = { (millimeter) : [10.0, 30.0, 70.0] } as LengthBoundSpec;
const MC_ARMTIP_BOUNDS = { (millimeter) : [6.0, 16.0, 50.0] } as LengthBoundSpec;
const MC_HUBR_BOUNDS = { (millimeter) : [20.0, 38.0, 90.0] } as LengthBoundSpec;

const MC_CLUTCHZ_BOUNDS = { (millimeter) : [25.0, 54.0, 100.0] } as LengthBoundSpec;
const MC_CLUTCHOD_BOUNDS = { (millimeter) : [35.0, 42.0, 130.0] } as LengthBoundSpec;
const MC_CLUTCHID_BOUNDS = { (millimeter) : [16.0, 28.0, 100.0] } as LengthBoundSpec;
const MC_TONGUET_BOUNDS = { (millimeter) : [5.0, 12.0, 30.0] } as LengthBoundSpec;
const MC_EART_BOUNDS = { (millimeter) : [4.0, 12.0, 25.0] } as LengthBoundSpec;
const MC_JOINTCLR_BOUNDS = { (millimeter) : [0.1, 0.35, 1.5] } as LengthBoundSpec;
const MC_TOOTHCLR_BOUNDS = { (millimeter) : [0.0, 0.15, 0.8] } as LengthBoundSpec;
const MC_BOLTD_BOUNDS = { (millimeter) : [3.0, 6.2, 12.0] } as LengthBoundSpec;
const MC_NUTAF_BOUNDS = { (millimeter) : [5.0, 10.3, 20.0] } as LengthBoundSpec;
const MC_NUTDEP_BOUNDS = { (millimeter) : [2.0, 4.0, 14.0] } as LengthBoundSpec;
const MC_SLEEVED_BOUNDS = { (millimeter) : [0.0, 0.0, 18.0] } as LengthBoundSpec;

const MC_AZZ_BOUNDS = { (millimeter) : [55.0, 85.0, 150.0] } as LengthBoundSpec;

const MC_AZFLANGE_BOUNDS = { (millimeter) : [18.0, 36.0, 70.0] } as LengthBoundSpec;
const MC_AZFLANGEH_BOUNDS = { (millimeter) : [5.0, 12.0, 30.0] } as LengthBoundSpec;
const MC_AZBOLTR_BOUNDS = { (millimeter) : [12.0, 30.0, 60.0] } as LengthBoundSpec;
const MC_AZBOLTD_BOUNDS = { (millimeter) : [2.5, 4.4, 9.0] } as LengthBoundSpec;
const MC_AZINSD_BOUNDS = { (millimeter) : [3.0, 5.8, 11.0] } as LengthBoundSpec;
const MC_AZINSDEP_BOUNDS = { (millimeter) : [3.0, 8.0, 18.0] } as LengthBoundSpec;
const MC_AZBOLTN_BOUNDS = { (unitless) : [2, 3, 8] } as IntegerBoundSpec;
const MC_MASTOD_BOUNDS = { (millimeter) : [22.0, 40.0, 70.0] } as LengthBoundSpec;
const MC_MASTBORE_BOUNDS = { (millimeter) : [7.0, 20.0, 34.0] } as LengthBoundSpec;
const MC_NPADZ_BOUNDS = { (millimeter) : [80.0, 145.0, 250.0] } as LengthBoundSpec;
const MC_NPADR_BOUNDS = { (millimeter) : [10.0, 22.0, 60.0] } as LengthBoundSpec;
const MC_NPADT_BOUNDS = { (millimeter) : [2.0, 6.0, 14.0] } as LengthBoundSpec;
const MC_NHOLE_BOUNDS = { (millimeter) : [5.0, 16.6, 28.0] } as LengthBoundSpec;
const MC_NFLAT_BOUNDS = { (millimeter) : [4.0, 14.0, 28.0] } as LengthBoundSpec;
const MC_CAPT_BOUNDS = { (millimeter) : [4.0, 9.0, 22.0] } as LengthBoundSpec;
const MC_CAPBOLTR_BOUNDS = { (millimeter) : [8.0, 16.0, 32.0] } as LengthBoundSpec;
const MC_CAPBOLTD_BOUNDS = { (millimeter) : [2.5, 3.4, 9.0] } as LengthBoundSpec;
const MC_CAPINSD_BOUNDS = { (millimeter) : [3.0, 5.0, 11.0] } as LengthBoundSpec;
const MC_CAPINSDEP_BOUNDS = { (millimeter) : [3.0, 6.0, 18.0] } as LengthBoundSpec;
const MC_CAPBOLTN_BOUNDS = { (unitless) : [2, 3, 8] } as IntegerBoundSpec;
const MC_NTHREAD_BOUNDS = { (millimeter) : [4.0, 19.8, 45.0] } as LengthBoundSpec;
const MC_NSPOT_BOUNDS = { (millimeter) : [0.0, 0.0, 6.0] } as LengthBoundSpec;

const MC_REACH_BOUNDS = { (millimeter) : [50.0, 105.0, 200.0] } as LengthBoundSpec;
const MC_PADZ_BOUNDS = { (millimeter) : [50.0, 116.0, 200.0] } as LengthBoundSpec;
const MC_STRUTW_BOUNDS = { (millimeter) : [10.0, 24.0, 60.0] } as LengthBoundSpec;
const MC_STRUTDROP_BOUNDS = { (millimeter) : [0.0, 14.0, 40.0] } as LengthBoundSpec;
const MC_STRUTRAMP_BOUNDS = { (millimeter) : [6.0, 12.0, 40.0] } as LengthBoundSpec;
const MC_COAXRAMP_BOUNDS = { (millimeter) : [8.0, 16.0, 60.0] } as LengthBoundSpec;
const MC_STRUTH_BOUNDS = { (millimeter) : [10.0, 32.0, 60.0] } as LengthBoundSpec;
const MC_COAXW_BOUNDS = { (millimeter) : [0.0, 16.0, 28.0] } as LengthBoundSpec;
const MC_COAXSTOP_BOUNDS = { (millimeter) : [0.0, 38.0, 120.0] } as LengthBoundSpec;

const MC_IFW_BOUNDS = { (millimeter) : [30.0, 80.0, 140.0] } as LengthBoundSpec;
const MC_IFL_BOUNDS = { (millimeter) : [30.0, 68.0, 180.0] } as LengthBoundSpec;
const MC_IFBX_BOUNDS = { (millimeter) : [15.0, 64.0, 120.0] } as LengthBoundSpec;
const MC_IFBY_BOUNDS = { (millimeter) : [15.0, 26.0, 160.0] } as LengthBoundSpec;
const MC_IFBD_BOUNDS = { (millimeter) : [2.5, 4.4, 9.0] } as LengthBoundSpec;
const MC_IFHEADD_BOUNDS = { (millimeter) : [4.0, 7.0, 20.0] } as LengthBoundSpec;
const MC_IFBSH_BOUNDS = { (millimeter) : [0.0, 10.0, 60.0] } as LengthBoundSpec;
const MC_HCBD_BOUNDS = { (millimeter) : [6.0, 11.0, 22.0] } as LengthBoundSpec;
const MC_HCBDEP_BOUNDS = { (millimeter) : [2.0, 6.3, 14.0] } as LengthBoundSpec;

// How steep an overhang the pad's tapered ears are allowed to print at,
// measured from vertical. The taper is not styling - see the pad. 40 deg is
// inside what ASA holds with part cooling; 45 is the usual hard limit.
const MC_PAD_OVERHANG = 40.0;
const MC_INSD_BOUNDS = { (millimeter) : [2.5, 5.8, 11.0] } as LengthBoundSpec;
const MC_INSDEP_BOUNDS = { (millimeter) : [2.0, 8.0, 15.0] } as LengthBoundSpec;
const MC_IFPADT_BOUNDS = { (millimeter) : [3.0, 6.0, 18.0] } as LengthBoundSpec;

const MC_GUSSETD_BOUNDS = { (millimeter) : [0.0, 14.0, 40.0] } as LengthBoundSpec;
const MC_GUSSETT_BOUNDS = { (millimeter) : [3.0, 8.0, 20.0] } as LengthBoundSpec;
const MC_EDGEBREAK_BOUNDS = { (millimeter) : [0.0, 0.8, 4.0] } as LengthBoundSpec;
const MC_BLEND_BOUNDS = { (millimeter) : [0.0, 3.0, 12.0] } as LengthBoundSpec;
const MC_KNEE_BOUNDS = { (millimeter) : [0.0, 0.0, 90.0] } as LengthBoundSpec;
const MC_FLAREW_BOUNDS = { (millimeter) : [0.0, 48.0, 120.0] } as LengthBoundSpec;
const MC_TETHERD_BOUNDS = { (millimeter) : [0.0, 7.0, 16.0] } as LengthBoundSpec;

const MC_ARMS_BOUNDS = { (unitless) : [3, 4, 10] } as IntegerBoundSpec;
const MC_PERARM_BOUNDS = { (unitless) : [1, 2, 4] } as IntegerBoundSpec;
const MC_CTEETH_BOUNDS = { (unitless) : [8, 36, 240] } as IntegerBoundSpec;
const MC_AZSTEPS_BOUNDS = { (unitless) : [6, 24, 120] } as IntegerBoundSpec;

const MC_TILT_BOUNDS = { (degree) : [0.0, 0.0, 60.0] } as AngleBoundSpec;
const MC_AZPREV_BOUNDS = { (degree) : [-180.0, 0.0, 180.0] } as AngleBoundSpec;
const MC_PANELTILT_BOUNDS = { (degree) : [0.0, 45.0, 85.0] } as AngleBoundSpec;
const MC_TILTMAX_BOUNDS = { (degree) : [5.0, 30.0, 90.0] } as AngleBoundSpec;

// ------------------------------------------------------------- helpers
function mm(v)
{
    return v * millimeter;
}

function p3(x, y, z)
{
    return vector(x, y, z) * millimeter;
}

function block(context is Context, id is Id, name is string, c1 is array, c2 is array) returns Query
{
    fCuboid(context, id + name, {
                "corner1" : p3(c1[0], c1[1], c1[2]),
                "corner2" : p3(c2[0], c2[1], c2[2])
            });
    return qCreatedBy(id + name, EntityType.BODY);
}

// Cylinder along Z.
function cylZ(context is Context, id is Id, name is string, x, y, z0, z1, r) returns Query
{
    fCylinder(context, id + name, {
                "bottomCenter" : p3(x, y, z0),
                "topCenter" : p3(x, y, z1),
                "radius" : mm(r)
            });
    return qCreatedBy(id + name, EntityType.BODY);
}

// Cylinder along Y - the clutch axis.
function cylY(context is Context, id is Id, name is string, x, z, y0, y1, r) returns Query
{
    fCylinder(context, id + name, {
                "bottomCenter" : p3(x, y0, z),
                "topCenter" : p3(x, y1, z),
                "radius" : mm(r)
            });
    return qCreatedBy(id + name, EntityType.BODY);
}

// UNION and SUBTRACTION keep the target's identity; INTERSECTION does not.
// Returning qUnion of every input means the survivor is found whichever
// identity won the boolean.
function unionInto(context is Context, id is Id, name is string, target is Query, tools is array) returns Query
{
    if (size(tools) == 0)
        return target;
    const all = concatenateArrays([[target], tools]);
    opBoolean(context, id + name, {
                "tools" : qUnion(all),
                "operationType" : BooleanOperationType.UNION
            });
    return qUnion(all);
}

function cutWith(context is Context, id is Id, name is string, targets is Query, tools is array)
{
    if (size(tools) == 0)
        return;
    opBoolean(context, id + name, {
                "targets" : targets,
                "tools" : qUnion(tools),
                "operationType" : BooleanOperationType.SUBTRACTION
            });
}

// Hexagonal prism along Z, across-flats = af. A block with its corners
// trimmed off: subtraction preserves the target's identity, so the query
// returned here always finds the result. Building it as an INTERSECTION
// would hand back a body this query cannot see, and the tool would survive
// the later cut as a stray solid.
function hexPrismZ(context is Context, id is Id, name is string, cx, cy, z0, z1, af) returns Query
{
    const r = af / 2.0;
    const big = af;
    var body = block(context, id, name ~ "b",
            [cx - big, cy - big, z0], [cx + big, cy + big, z1]);
    var trims = [];
    for (var k = 0; k < 6; k += 1)
    {
        const t = block(context, id, name ~ "t" ~ toString(k),
                [cx - 2 * big, cy + r, z0 - 1], [cx + 2 * big, cy + 3 * big, z1 + 1]);
        opTransform(context, id + (name ~ "tr" ~ toString(k)), {
                    "bodies" : t,
                    "transform" : rotationAround(line(p3(cx, cy, 0), vector(0, 0, 1)),
                            (k * 60.0) * degree)
                });
        trims = append(trims, t);
    }
    cutWith(context, id, name ~ "cut", body, trims);
    return body;
}

// ---------------------------------------------------------- tooth rings
//
// A face spline generated the only way that is guaranteed to mate: BOTH
// halves get the same zigzag, offset by half a pitch, so peaks drop into
// valleys. No complement solid, no Boolean of one half against the other.
//
// One cutter is a square prism rolled 45 degrees about its own axis, so its
// lower half presents a 90 degree V. It spans the full chord, which means
// half a turn of copies produces the whole ring.
//
// Tooth DEPTH is derived, never typed, and it is sized off the OUTSIDE
// DIAMETER - not the mean radius.
//
// Sized at the mean radius, the cutter's width at the face equals the pitch
// EXACTLY there, so adjacent grooves merely TOUCH and the surface necks to
// zero width along one line per tooth. On a small tooth boss the kernel
// tolerated that; once the face became the head's whole flat bottom, shared
// with the flange and the arm, it produced "Boolean operation would result in
// non-manifold body".
//
// Sized off the OD with 3% over, adjacent cutters OVERLAP at every radius in
// the annulus, so the surface is a continuous zigzag that never touches
// itself. The overlap IS the running clearance - the halves sit
// (overlap + toothClear) apart - so it is kept small, and contact concentrates
// at the outer radius, which is where the torque is anyway.
//
// Plain millimetre numbers in, plain millimetre number out - the whole feature
// body works that way, and only the helpers that touch geometry apply units.
const MC_RING_EXTEND = 1.0;   // how far the cutter runs past the OD

function toothDepth(od is number, idia is number, count is number) returns number
{
    // Derived from the radius the cutter ACTUALLY reaches, not the OD. Sizing
    // it off the OD and then extending the cutter past the OD pulls the
    // width-equals-pitch radius back inside the cutter's own range, which is
    // the tangency this is meant to avoid.
    return PI * (od / 2.0 + MC_RING_EXTEND) / count * 1.03;
}

function vToothRing(context is Context, id is Id, name is string,
    count is number, od, idia, depth, spinDeg) returns Query
{
    const s = depth * sqrt(2.0);
    const r0 = max(idia / 2.0 - 1.0, 0.6);
    // Runs 1 mm PAST the OD so each prism's end face exits the body it is
    // cutting. Ending exactly at od/2 puts that face on the part's own
    // cylindrical surface - a tangent contact, and the fatter the teeth get the
    // bigger it is. The mating boss is still only od/2, so the extra groove is
    // just clearance.
    const r1 = od / 2.0 + MC_RING_EXTEND;

    // A RADIAL SEGMENT, from just inside the ID out to the OD - NOT a full
    // chord. Full chords all run through the axis, so half the ring's bodies
    // pile up inside one sub-millimetre blob at r = 0. Onshape fails that pile
    // and reports only "Boolean operation failed to return a valid part",
    // naming neither the operation nor the reason. Starting at r0 also means
    // one cutter per tooth rather than one per two, which costs nothing.
    fCuboid(context, id + (name ~ "seed"), {
                "corner1" : p3(r0, -s / 2.0, -s / 2.0),
                "corner2" : p3(r1, s / 2.0, s / 2.0)
            });
    const seed = qCreatedBy(id + (name ~ "seed"), EntityType.BODY);

    // Roll 45 degrees: the square becomes a diamond whose lower vertex sits
    // at -depth and whose waist is exactly at z = 0, the face plane.
    opTransform(context, id + (name ~ "roll"), {
                "bodies" : seed,
                "transform" : rotationAround(line(p3(0, 0, 0), vector(1, 0, 0)), 45 * degree)
            });

    if (spinDeg != 0)
    {
        opTransform(context, id + (name ~ "spin"), {
                    "bodies" : seed,
                    "transform" : rotationAround(line(p3(0, 0, 0), vector(0, 0, 1)), spinDeg * degree)
                });
    }

    var xf = [];
    var nm = [];
    for (var k = 1; k < count; k += 1)
    {
        xf = append(xf, rotationAround(line(p3(0, 0, 0), vector(0, 0, 1)),
                    (k * 360.0 / count) * degree));
        nm = append(nm, name ~ "p" ~ toString(k));
    }
    var all = [seed];
    if (size(xf) > 0)
    {
        opPattern(context, id + (name ~ "pat"), {
                    "entities" : seed,
                    "transforms" : xf,
                    "instanceNames" : nm
                });
        all = append(all, qCreatedBy(id + (name ~ "pat"), EntityType.BODY));
    }
    return qUnion(all);
}

// Move a ring built at the origin with its axis along +Z onto a face whose
// outward normal is one of +Z, +Y or -Y. rotX is what maps local +Z there.
function placeRing(context is Context, id is Id, name is string, ring is Query,
    rotXDeg, at is Vector)
{
    if (rotXDeg != 0)
    {
        opTransform(context, id + (name ~ "rx"), {
                    "bodies" : ring,
                    "transform" : rotationAround(line(p3(0, 0, 0), vector(1, 0, 0)), rotXDeg * degree)
                });
    }
    opTransform(context, id + (name ~ "mv"), {
                "bodies" : ring,
                "transform" : transform(at)
            });
}

// Edge picked by WHERE IT IS, not by what made it. After this many booleans a
// qCreatedBy edge query is worthless; a horizontal ring at a known height
// survives everything.
function edgeIsHorizontalAt(context is Context, e is Query, zTarget) returns boolean
{
    const l = evEdgeTangentLine(context, { "edge" : e, "parameter" : 0.5 });
    return abs(l.direction[2]) < 0.01 && abs(l.origin[2] / millimeter - zTarget) < 0.05;
}

// Pick an edge by WHERE it is: vertical, on a side face, at one end. The four
// outer corners of a plate and nothing else.
function edgeIsOuterCorner(context is Context, e is Query, xa, xb, yHalf, tol) returns boolean
{
    const l = evEdgeTangentLine(context, { "edge" : e, "parameter" : 0.5 });
    if (abs(l.direction[0]) > 0.01 || abs(l.direction[1]) > 0.01)
        return false;
    const x = l.origin[0] / millimeter;
    const y = l.origin[1] / millimeter;
    return abs(abs(y) - yHalf) < tol && (abs(x - xa) < tol || abs(x - xb) < tol);
}

function chamferEdgeSet(context is Context, id is Id, edges is Query, amt) returns boolean
{
    opChamfer(context, id, {
                "entities" : edges,
                "chamferType" : ChamferType.EQUAL_OFFSETS,
                "width" : mm(amt)
            });
    return true;
}

// Break or round the ring of edges at one height. The WHOLE ring must go in a
// single operation: done one edge at a time, each corner arc then has to blend
// into faces that already exist at both its ends, and it fails. The per-edge
// loop is only a fallback for when the ring genuinely will not take the size.
function ringAt(context is Context, id is Id, name is string, body is Query,
    zTarget, amt, asFillet is boolean, minLen)
{
    if (amt <= 0)
        return;
    var picks = [];
    for (var e in evaluateQuery(context, body->qOwnedByBody(EntityType.EDGE)))
    {
        const ok = try silent(edgeIsHorizontalAt(context, e, zTarget));
        if (ok != true)
            continue;
        // Skip small rims. A height filter alone also catches every hole mouth
        // that opens at that height - counterbores, magnet pockets, the N hole.
        // None of those want rounding, and a 3 mm fillet on a counterbore rim
        // sitting 1.6 mm from the flange edge cannot be built at all: the ring
        // op fails and so does the per-edge fallback.
        const len = try silent(evLength(context, { "entities" : e }));
        if (len == undefined || len < mm(minLen))
            continue;
        picks = append(picks, e);
    }
    if (size(picks) == 0)
        return;

    var one = false;
    if (asFillet)
        one = try silent(filletEdgeSet(context, id + (name ~ "r"), qUnion(picks), amt)) == true;
    else
        one = try silent(chamferEdgeSet(context, id + (name ~ "r"), qUnion(picks), amt)) == true;
    if (one)
        return;

    for (var i = 0; i < size(picks); i += 1)
    {
        if (asFillet)
            try silent(filletEdgeSet(context, id + (name ~ toString(i)), picks[i], amt));
        else
            try silent(chamferEdgeSet(context, id + (name ~ toString(i)), picks[i], amt));
    }
}

// opFillet returns nothing, so a bare `try silent` around it hides every
// failure. Wrapping it in a function that returns true gives a value to test.
function filletEdgeSet(context is Context, id is Id, edges is Query, amt) returns boolean
{
    opFillet(context, id, { "entities" : edges, "radius" : mm(amt) });
    return true;
}

// ================================================= FEATURE 1: THE MOUNT
annotation { "Feature Type Name" : "MeshCore mag mount" }
export const meshcoreMagMount = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Name" : "Build" }
        definition.parts is MountParts;

        annotation { "Group Name" : "Magnet base", "Collapsed By Default" : false }
        {
            annotation { "Name" : "Base style" }
            definition.baseStyle is BaseStyle;

            annotation { "Name" : "Round base - disc diameter" }
            isLength(definition.roundD, MC_ROUNDD_BOUNDS);

            annotation { "Name" : "Round base - disc thickness" }
            isLength(definition.roundT, MC_ROUNDT_BOUNDS);

            annotation { "Name" : "Round base - magnet circle radius" }
            isLength(definition.roundMagR, MC_ROUNDMAGR_BOUNDS);

            annotation { "Name" : "Round base - magnet count" }
            isInteger(definition.roundMags, MC_ROUNDMAGS_BOUNDS);

            annotation { "Name" : "Cross base - arms" }
            isInteger(definition.magArms, MC_ARMS_BOUNDS);

            annotation { "Name" : "Magnets per arm tip" }
            isInteger(definition.magPerArm, MC_PERARM_BOUNDS);

            annotation { "Name" : "Arm radius (magnet circle)" }
            isLength(definition.magR, MC_MAGR_BOUNDS);

            annotation { "Name" : "Tangential spacing within a pair" }
            isLength(definition.magPitch, MC_MAGPITCH_BOUNDS);

            annotation { "Name" : "Magnet pocket diameter" }
            isLength(definition.magD, MC_MAGD_BOUNDS);

            annotation { "Name" : "Magnet thickness" }
            isLength(definition.magT, MC_MAGT_BOUNDS);

            annotation { "Name" : "Steel yoke thickness (0 = none)" }
            isLength(definition.yokeT, MC_YOKET_BOUNDS);

            annotation { "Name" : "Material over the yoke" }
            isLength(definition.magWall, MC_MAGWALL_BOUNDS);

            annotation { "Name" : "Push-out hole - OPENS THE POCKET TO RAIN (0 = none)" }
            isLength(definition.pushD, MC_PUSHD_BOUNDS);

            annotation { "Name" : "Arm width" }
            isLength(definition.armW, MC_ARMW_BOUNDS);

            annotation { "Name" : "Arm height at the hub" }
            isLength(definition.armRootH, MC_ARMROOT_BOUNDS);

            annotation { "Name" : "Arm height at the tip" }
            isLength(definition.armTipH, MC_ARMTIP_BOUNDS);

            annotation { "Name" : "Hub radius" }
            isLength(definition.hubR, MC_HUBR_BOUNDS);

            annotation { "Name" : "Tether eye diameter (0 = none)" }
            isLength(definition.tetherD, MC_TETHERD_BOUNDS);
        }

        annotation { "Group Name" : "Tilt clutch", "Collapsed By Default" : true }
        {
            annotation { "Name" : "Clutch axis height" }
            isLength(definition.clutchZ, MC_CLUTCHZ_BOUNDS);

            annotation { "Name" : "Tooth ring outside diameter" }
            isLength(definition.clutchOD, MC_CLUTCHOD_BOUNDS);

            annotation { "Name" : "Tooth ring inside diameter" }
            isLength(definition.clutchID, MC_CLUTCHID_BOUNDS);

            annotation { "Name" : "Tooth count (360/count = step)" }
            isInteger(definition.clutchTeeth, MC_CTEETH_BOUNDS);

            annotation { "Name" : "Tooth flank clearance" }
            isLength(definition.toothClear, MC_TOOTHCLR_BOUNDS);

            annotation { "Name" : "Tongue thickness" }
            isLength(definition.tongueT, MC_TONGUET_BOUNDS);

            annotation { "Name" : "Clearance around the swinging tongue" }
            isLength(definition.jointClear, MC_JOINTCLR_BOUNDS);

            annotation { "Name" : "Ear thickness" }
            isLength(definition.earT, MC_EART_BOUNDS);

            annotation { "Name" : "Gusset depth behind the ear (0 = none)" }
            isLength(definition.gussetD, MC_GUSSETD_BOUNDS);

            annotation { "Name" : "Gusset thickness" }
            isLength(definition.gussetT, MC_GUSSETT_BOUNDS);

            annotation { "Name" : "Maximum tilt" }
            isAngle(definition.tiltMax, MC_TILTMAX_BOUNDS);

            annotation { "Name" : "Axis bolt clearance" }
            isLength(definition.boltD, MC_BOLTD_BOUNDS);

            // DEFAULTS TO 0, AND MUST. holeR below is sleeveD/2 when this is
            // set, so leaving it on bores the clutch axis to the SLEEVE size -
            // 10.2 mm for an M6 bolt - whether or not a sleeve was ever turned.
            // It shipped at 10.2 and did two things at once: the bolt rattled
            // in the joint, and the 10.3 A/F nut pocket was left with 1.6 mm2
            // of flat to bear on, which is nothing. Turn it on only when the
            // sleeve is actually in your hand.
            annotation { "Name" : "Turned sleeve outside diameter (0 = none)" }
            isLength(definition.sleeveD, MC_SLEEVED_BOUNDS);

            annotation { "Name" : "Axis nut" }
            definition.clutchNut is NutStyle;

            annotation { "Name" : "Nut across flats" }
            isLength(definition.nutAF, MC_NUTAF_BOUNDS);

            annotation { "Name" : "Nut pocket depth" }
            isLength(definition.nutDepth, MC_NUTDEP_BOUNDS);
        }

        annotation { "Group Name" : "Azimuth index", "Collapsed By Default" : true }
        {
            annotation { "Name" : "Azimuth face height" }
            isLength(definition.azZ, MC_AZZ_BOUNDS);

            // Not teeth any more - see the head. This is the number of index
            // HOLES in the post's flange, and 360/it is the azimuth step. It
            // must divide by the clamp bolt count or the bolts cannot all land
            // on holes at once.
            annotation { "Name" : "Azimuth index positions (360/count = step)" }
            isInteger(definition.azSteps, MC_AZSTEPS_BOUNDS);

            annotation { "Name" : "Clamp flange radius" }
            isLength(definition.azFlangeR, MC_AZFLANGE_BOUNDS);

            annotation { "Name" : "Clamp flange height" }
            isLength(definition.azFlangeH, MC_AZFLANGEH_BOUNDS);

            annotation { "Name" : "Clamp bolt circle radius" }
            isLength(definition.azBoltR, MC_AZBOLTR_BOUNDS);

            annotation { "Name" : "Clamp bolt count" }
            isInteger(definition.azBoltN, MC_AZBOLTN_BOUNDS);

            // M4, and the clearance and the insert bore have to name the SAME
            // thread - they shipped as an M3-ish clearance against an M4 insert
            // bore, which is why neither fitted. 5.8 is the MEASURED plain OD of
            // the M4 insert in stock (0.227 in), not a catalogue figure; the
            // knurls above it melt into the wall.
            // This is the most loaded bolted joint after the clutch - it carries
            // the whole head - and there is room for M4: the insert spans r 27.1
            // to 32.9 against a tooth ring at 25 and a flange edge at 36. M3
            // would also hold (212 N of worst-case bolt tension at 90 mph against
            // ~2.2 kN of proof load), but there is no reason to take the margin.
            annotation { "Name" : "Clamp bolt clearance diameter" }
            isLength(definition.azBoltD, MC_AZBOLTD_BOUNDS);

            annotation { "Name" : "Clamp insert hole diameter" }
            isLength(definition.azInsD, MC_AZINSD_BOUNDS);

            annotation { "Name" : "Clamp insert depth" }
            isLength(definition.azInsDep, MC_AZINSDEP_BOUNDS);
        }

        annotation { "Group Name" : "Mast and antenna", "Collapsed By Default" : true }
        {
            annotation { "Name" : "Mast outside diameter" }
            isLength(definition.mastOD, MC_MASTOD_BOUNDS);

            annotation { "Name" : "Mast bore - must pass the N connector body" }
            isLength(definition.mastBore, MC_MASTBORE_BOUNDS);

            annotation { "Name" : "N bulkhead face height" }
            isLength(definition.nPadZ, MC_NPADZ_BOUNDS);

            annotation { "Name" : "N pad radius" }
            isLength(definition.nPadR, MC_NPADR_BOUNDS);

            annotation { "Name" : "N pad thickness under the nut" }
            isLength(definition.nPadT, MC_NPADT_BOUNDS);

            annotation { "Name" : "N mounting hole - round diameter" }
            isLength(definition.nHole, MC_NHOLE_BOUNDS);

            annotation { "Name" : "N mounting hole - across the flats (0 = plain round)" }
            isLength(definition.nFlat, MC_NFLAT_BOUNDS);

            annotation { "Name" : "N threaded length (from the datasheet)" }
            isLength(definition.nThread, MC_NTHREAD_BOUNDS);

            annotation { "Name" : "Antenna cap thickness" }
            isLength(definition.capT, MC_CAPT_BOUNDS);

            annotation { "Name" : "Cap bolt circle radius" }
            isLength(definition.capBoltR, MC_CAPBOLTR_BOUNDS);

            annotation { "Name" : "Cap bolt count" }
            isInteger(definition.capBoltN, MC_CAPBOLTN_BOUNDS);

            annotation { "Name" : "Cap bolt clearance diameter" }
            isLength(definition.capBoltD, MC_CAPBOLTD_BOUNDS);

            annotation { "Name" : "Cap insert hole diameter" }
            isLength(definition.capInsD, MC_CAPINSD_BOUNDS);

            annotation { "Name" : "Cap insert depth" }
            isLength(definition.capInsDep, MC_CAPINSDEP_BOUNDS);

            annotation { "Name" : "Spot-face (buys thread 1:1)" }
            isLength(definition.nSpot, MC_NSPOT_BOUNDS);
        }

        annotation { "Group Name" : "Device arm", "Collapsed By Default" : true }
        {
            annotation { "Name" : "Arm reach from the mast axis" }
            isLength(definition.armReach, MC_REACH_BOUNDS);

            annotation { "Name" : "Interface pad centre height" }
            isLength(definition.padZ, MC_PADZ_BOUNDS);

            annotation { "Name" : "Panel tilt from horizontal" }
            isAngle(definition.panelTilt, MC_PANELTILT_BOUNDS);

            annotation { "Name" : "Strut width" }
            isLength(definition.strutW, MC_STRUTW_BOUNDS);

            annotation { "Name" : "Arm top dropped, inboard" }
            isLength(definition.strutDrop, MC_STRUTDROP_BOUNDS);

            annotation { "Name" : "Run the arm top climbs over" }
            isLength(definition.strutRamp, MC_STRUTRAMP_BOUNDS);

            annotation { "Name" : "Strut depth" }
            isLength(definition.strutH, MC_STRUTH_BOUNDS);

            annotation { "Name" : "Coax channel width (0 = none)" }
            isLength(definition.coaxW, MC_COAXW_BOUNDS);

            annotation { "Name" : "Coax leaves the arm this far before the pad" }
            isLength(definition.coaxStop, MC_COAXSTOP_BOUNDS);

            annotation { "Name" : "Coax exit ramp run" }
            isLength(definition.coaxRamp, MC_COAXRAMP_BOUNDS);
        }

        annotation { "Group Name" : "MC-TOP-1 interface", "Collapsed By Default" : true }
        {
            annotation { "Name" : "Pad width" }
            isLength(definition.ifW, MC_IFW_BOUNDS);

            annotation { "Name" : "Pad length" }
            isLength(definition.ifL, MC_IFL_BOUNDS);

            annotation { "Name" : "Bolt pattern across" }
            isLength(definition.ifBoltX, MC_IFBX_BOUNDS);

            annotation { "Name" : "Bolt pattern along" }
            isLength(definition.ifBoltY, MC_IFBY_BOUNDS);

            annotation { "Name" : "Bolt clearance diameter" }
            isLength(definition.ifBoltD, MC_IFBD_BOUNDS);

            // Cap head diameter, not a countersink. Nothing is bored for it -
            // the head bears straight on the pad's back face - but the edge
            // distance and the driver clearance are both measured off it.
            annotation { "Name" : "Bolt head diameter" }
            isLength(definition.ifHeadD, MC_IFHEADD_BOUNDS);

            annotation { "Name" : "Bolt pattern offset up-plane" }
            isLength(definition.ifBoltShift, MC_IFBSH_BOUNDS);

            annotation { "Name" : "Pad thickness" }
            isLength(definition.ifPadT, MC_IFPADT_BOUNDS);
        }

        annotation { "Group Name" : "Finish", "Collapsed By Default" : true }
        {
            annotation { "Name" : "Edge break (chamfer on exposed edges)" }
            isLength(definition.edgeBreak, MC_EDGEBREAK_BOUNDS);

            annotation { "Name" : "Blend radius at structural transitions" }
            isLength(definition.blendR, MC_BLEND_BOUNDS);

            annotation { "Name" : "Knee brace above the arm (0 = none)" }
            isLength(definition.kneeLen, MC_KNEE_BOUNDS);

            annotation { "Name" : "Arm flare width at the pad (0 = none)" }
            isLength(definition.flareW, MC_FLAREW_BOUNDS);
        }

        annotation { "Group Name" : "Preview only", "Collapsed By Default" : true }
        {
            annotation { "Name" : "Show at this tilt (print at 0)" }
            isAngle(definition.tiltPreview, MC_TILT_BOUNDS);

            annotation { "Name" : "Show at this azimuth (print at 0)" }
            isAngle(definition.azPreview, MC_AZPREV_BOUNDS);
        }
    }
    {
        const wantBase = definition.parts == MountParts.ALL || definition.parts == MountParts.BASE;
        const wantPost = definition.parts == MountParts.ALL || definition.parts == MountParts.YAW_POST;
        const wantHead = definition.parts == MountParts.ALL || definition.parts == MountParts.HEAD;

        // ---- unpack to plain millimetre numbers ----------------------
        const magR = definition.magR / millimeter;
        const magPitch = definition.magPitch / millimeter;
        const magD = definition.magD / millimeter;
        const magT = definition.magT / millimeter;
        const yokeT = definition.yokeT / millimeter;
        const magWall = definition.magWall / millimeter;
        const pushD = definition.pushD / millimeter;
        const armW = definition.armW / millimeter;
        const armRootH = definition.armRootH / millimeter;
        const armTipH = definition.armTipH / millimeter;
        const hubR = definition.hubR / millimeter;
        const tetherD = definition.tetherD / millimeter;
        const roundD = definition.roundD / millimeter;
        const roundT = definition.roundT / millimeter;
        const roundMagR = definition.roundMagR / millimeter;
        const roundMags = definition.roundMags;
        const isRound = definition.baseStyle == BaseStyle.ROUND;
        const nArms = definition.magArms;
        const nPer = definition.magPerArm;

        const clutchZ = definition.clutchZ / millimeter;
        const clutchOD = definition.clutchOD / millimeter;
        const clutchID = definition.clutchID / millimeter;
        const nTeeth = definition.clutchTeeth;
        const toothClear = definition.toothClear / millimeter;
        const tongueT = definition.tongueT / millimeter;
        const jointClear = definition.jointClear / millimeter;
        const earT = definition.earT / millimeter;
        const boltD = definition.boltD / millimeter;
        const sleeveD = definition.sleeveD / millimeter;
        const nutAF = definition.nutAF / millimeter;
        const nutDepth = definition.nutDepth / millimeter;
        const gussetD = definition.gussetD / millimeter;
        const gussetT = definition.gussetT / millimeter;
        const tiltMax = definition.tiltMax / degree;
        const edgeBreak = definition.edgeBreak / millimeter;
        const blendR = definition.blendR / millimeter;
        const kneeLen = definition.kneeLen / millimeter;
        const flareW = definition.flareW / millimeter;

        const azZ = definition.azZ / millimeter;

        const azN = definition.azSteps;

        const azFlangeR = definition.azFlangeR / millimeter;
        const azFlangeH = definition.azFlangeH / millimeter;
        const azBoltR = definition.azBoltR / millimeter;
        const azBoltN = definition.azBoltN;
        const azBoltD = definition.azBoltD / millimeter;
        const azInsD = definition.azInsD / millimeter;
        const azInsDep = definition.azInsDep / millimeter;

        const mastOD = definition.mastOD / millimeter;
        const mastBore = definition.mastBore / millimeter;
        const nPadZ = definition.nPadZ / millimeter;
        const nPadR = definition.nPadR / millimeter;
        const nPadT = definition.nPadT / millimeter;
        const nHole = definition.nHole / millimeter;
        const nFlat = definition.nFlat / millimeter;
        const nThread = definition.nThread / millimeter;
        const capT = definition.capT / millimeter;
        const capBoltR = definition.capBoltR / millimeter;
        const capBoltN = definition.capBoltN;
        const capBoltD = definition.capBoltD / millimeter;
        const capInsD = definition.capInsD / millimeter;
        const capInsDep = definition.capInsDep / millimeter;
        const nSpot = definition.nSpot / millimeter;

        const armReach = definition.armReach / millimeter;
        const padZ = definition.padZ / millimeter;
        const panelTilt = definition.panelTilt / degree;
        const strutW = definition.strutW / millimeter;
        const strutDrop = definition.strutDrop / millimeter;
        const strutRamp = definition.strutRamp / millimeter;
        const strutH = definition.strutH / millimeter;
        const coaxW = definition.coaxW / millimeter;
        const coaxStop = definition.coaxStop / millimeter;
        const coaxRamp = definition.coaxRamp / millimeter;

        const ifW = definition.ifW / millimeter;
        const ifL = definition.ifL / millimeter;
        const ifBoltX = definition.ifBoltX / millimeter;
        const ifBoltY = definition.ifBoltY / millimeter;
        const ifBoltD = definition.ifBoltD / millimeter;
        const ifHeadD = definition.ifHeadD / millimeter;
        const ifBoltShift = definition.ifBoltShift / millimeter;
        const ifPadT = definition.ifPadT / millimeter;

        // ---- derived, and the sanity checks that go with them --------
        const cDepth = toothDepth(clutchOD, clutchID, nTeeth);
        const cStep = 360.0 / nTeeth;
        const aStep = 360.0 / azN;

        const steps = floor(tiltMax / cStep);
        reportFeatureInfo(context, id,
            "Clutch: " ~ toString(nTeeth) ~ " teeth, " ~ toString(cStep) ~ " deg step, "
            ~ toString(cDepth) ~ " mm deep, " ~ toString(steps) ~ " steps to "
            ~ toString(tiltMax) ~ " deg.  Azimuth: " ~ toString(azN) ~ " index holes, "
            ~ toString(aStep) ~ " deg step, no teeth - the bolts index it.  "
            ~ "Head skirt " ~ toString(2.0 * azFlangeR) ~ " mm dia.");

        // no tangency: the cutter must be WIDER than the pitch at the OD
        if (2.0 * cDepth <= 2.0 * PI * (clutchOD / 2.0 + MC_RING_EXTEND) / nTeeth)
            reportFeatureWarning(context, id,
                "Tooth cutters only just reach the pitch at the OD - adjacent grooves "
                ~ "would touch rather than overlap, which is a non-manifold surface.");
        if (cDepth < 0.5)
            reportFeatureWarning(context, id,
                "Teeth under 0.5 mm deep will not survive slicing. Drop the tooth count or grow the ring.");
        if (clutchID >= clutchOD)
            throw regenError("Tooth ring ID must be smaller than its OD.", ["clutchID"]);
        // The channel starts at the mast surface and rises above the knee brace.
        // Both of those can reach the N flange and the cap inserts behind it.
        if (mastOD / 2.0 + 1.0 <= capBoltR + capInsD / 2.0)
            reportFeatureWarning(context, id,
                "Coax channel starts inside the cap insert circle - it will open into "
                ~ "the insert holes. Fill the mast out or pull the inserts in.");
        // Dropping the arm drops the channel and the mast outlet with it, and
        // the outlet is the one that runs out of room first - it has to stay
        // clear of the azimuth clamp flange it would otherwise cut into.
        if (padZ + strutH / 2.0 - strutDrop - coaxW * 0.8 - 3.0 < azZ + azFlangeH + 2.0)
            reportFeatureWarning(context, id,
                "The arm is dropped so far that the coax outlet would cut into "
                ~ "the azimuth clamp flange. Reduce the drop or the channel width.");
        if (padZ + strutH / 2.0 - strutDrop + kneeLen + 2.0 > nPadZ - nPadT - 4.0)
            reportFeatureWarning(context, id,
                "Coax channel rises past the bottom of the N flange and will notch it. "
                ~ "Shorten the knee brace or raise nPadZ.");
        if (isRound)
        {
            if (roundT < magT + yokeT + magWall)
                reportFeatureWarning(context, id,
                    "Round disc is thinner than the magnet pocket plus its cover.");
            if (roundMagR + magD / 2.0 + 2.0 > roundD / 2.0)
                reportFeatureWarning(context, id,
                    "Magnet circle runs off the edge of the disc.");
            if (PI * 2.0 * roundMagR / roundMags < magD + 2.0)
                reportFeatureWarning(context, id,
                    "Magnets are too close together on the circle.");
            if (roundMagR - magD / 2.0 - 2.0 < hubR)
                reportFeatureWarning(context, id,
                    "Magnet circle runs under the hub - pockets would break into it.");
        }
        // ---- STALE-INSTANCE TRAP ----------------------------------------
        // Onshape back-fills only parameters that did NOT exist before. One that
        // already exists keeps its stored value forever, whatever the defaults
        // map now says. So a feature placed before the head was redesigned is
        // still holding the old padZ, ifL and kneeLen - and the new flat-bottom
        // code driven by those old numbers builds a pad hanging 18 mm below the
        // bottom plane and 23 mm past the end of the arm.
        //
        // The only symptom Onshape gives for that is "Boolean operation would
        // result in non-manifold body", which says nothing about the cause. Fail
        // loudly and name the cure instead.
        if (padZ - (ifL / 2.0 + ifPadT) * cos(panelTilt * degree) < azZ)
            throw regenError("The interface pad hangs below the head's bottom "
                ~ "plane. This almost always means the feature was placed before "
                ~ "the head was redesigned and is still holding its OLD parameter "
                ~ "values - changing a default never updates an existing feature. "
                ~ "DELETE THIS FEATURE AND RE-ADD IT.", ["padZ", "ifL", "ifPadT"]);
        if (armReach + (ifL / 2.0) * cos(panelTilt * degree)
                > armReach + (padZ - azZ) - 4.0)
            throw regenError("The interface pad reaches past the end of the arm. "
                ~ "Same cause: an instance still holding old parameter values. "
                ~ "DELETE THIS FEATURE AND RE-ADD IT.", ["padZ", "ifL"]);
        // Third face of the same trap, and the one the ear redesign introduced.
        // ifBoltShift and ifHeadD are NEW parameters, so an existing instance
        // back-fills them correctly from their bound specs - but it keeps its OLD
        // ifL and ifW, and a 68 mm pattern on a 42 mm pad puts two countersinks
        // clean off the end of the pad, where they cut nothing and the boolean
        // has no idea what you meant.
        if (ifBoltShift + ifBoltY / 2.0 + ifHeadD / 2.0 > ifL / 2.0)
            throw regenError("The interface bolts fall off the end of the pad. "
                ~ "If this appeared on its own, the feature is still holding its "
                ~ "OLD pad size against the new bolt pattern - changing a default "
                ~ "never updates an existing feature. "
                ~ "DELETE THIS FEATURE AND RE-ADD IT.", ["ifL", "ifBoltY", "ifBoltShift"]);
        if (ifBoltX / 2.0 + ifHeadD / 2.0 > ifW / 2.0)
            throw regenError("The interface bolts fall off the side of the pad. "
                ~ "Same cause. DELETE THIS FEATURE AND RE-ADD IT.",
                ["ifW", "ifBoltX"]);

        // The arm must not feather out to nothing where the pad plane exits its
        // underside, and the pad's low corner must land ON the arm, not past it.
        if (padZ - azZ < 12.0)
            reportFeatureWarning(context, id,
                "Pad is too close to the bottom plane - the arm has no depth to end on.");
        // ifL / 2.0, NOT ifL / 2.0 + ifPadT. The pad's furthest point in +X is on
        // the MATING face, where lz = 0; adding the thickness there describes a
        // corner that does not exist and overstates the reach by ifPadT*cos.
        // The z test above is the one that legitimately carries + ifPadT, because
        // the pad's LOWEST point really is the back face's down-plane corner.
        // Getting the two mixed up fired this warning at the defaults.
        if (armReach + (ifL / 2.0) * cos(panelTilt * degree)
                > armReach + (padZ - azZ) - 5.0)
            reportFeatureWarning(context, id,
                "Interface pad reaches past the end of the arm - its low corner would "
                ~ "overhang. Shorten ifL or raise padZ.");
        if (nThread < nPadT + 5.0)
            reportFeatureWarning(context, id,
                "N thread is too short for this panel thickness plus a nut. "
                ~ "Thin the cap at the hole or raise nSpot.");
        if (mastBore < nHole + 2.0)
            reportFeatureWarning(context, id,
                "Mast bore will not pass the N connector body. The bulkhead installs "
                ~ "from underneath, so it has to fit up the bore.");
        if (nFlat > 0.01 && nFlat >= nHole)
            reportFeatureWarning(context, id,
                "N flats are wider than the hole diameter - there would be no flats.");
        // The three clamp bolts have to land on three of the index holes at
        // every tooth position, which only works if the tooth count divides by
        // the bolt count.
        if (azN != floor(azN / azBoltN) * azBoltN)
            reportFeatureWarning(context, id,
                "Azimuth teeth (" ~ toString(azN) ~ ") do not divide by the clamp "
                ~ "bolt count (" ~ toString(azBoltN) ~ "), so the bolts cannot all "
                ~ "land on index holes. Azimuth would only adjust in "
                ~ toString(360.0 / azBoltN) ~ " degree steps.");
        // ...and the index holes must leave web between them.
        if (2.0 * PI * azBoltR / azN - azBoltD < 3.0)
            reportFeatureWarning(context, id,
                "Only " ~ toString(2.0 * PI * azBoltR / azN - azBoltD) ~ " mm of "
                ~ "web between index holes. Use fewer azimuth teeth, a bigger bolt "
                ~ "circle, or a smaller bolt.");
        if (azBoltR - max(azBoltD, azInsD) / 2.0 - 2.0 < mastBore / 2.0)
            reportFeatureWarning(context, id,
                "Azimuth clamp bolts overlap the mast bore. Raise azBoltR.");
        if (azBoltR + max(azBoltD, azInsD) / 2.0 + 3.0 > azFlangeR)
            reportFeatureWarning(context, id,
                "Azimuth clamp bolts run off the flange. Raise azFlangeR.");
        // The tongue is a disc in a cylindrical pocket: it can only be inserted
        // along the clutch axis. If it dips into the hub, the hub walls that path
        // off and the post cannot be fitted at all.
        if (clutchZ - clutchOD / 2.0 < armRootH)
            reportFeatureWarning(context, id,
                "The clutch tongue dips into the hub, so it cannot be slid onto the "
                ~ "ear - the hub blocks the only insertion path. Raise clutchZ or "
                ~ "shrink clutchOD.");
        if (azZ < clutchZ + clutchOD / 2.0 + 8.0)
            reportFeatureWarning(context, id,
                "Less than 8 mm between the clutch ear tops and the azimuth face. "
                ~ "The yaw post's neck has to stay as thin as the tongue until it "
                ~ "clears the ears, so it has nowhere to flare. Raise azZ or shrink clutchOD.");

        const padHalfR = magD / 2.0 + magWall;          // radial half-size of a tip pad
        const padHalfT = (nPer - 1) * magPitch / 2.0 + magD / 2.0 + magWall;
        const pocketTop = magT + yokeT;                 // pocket runs 0 .. pocketTop

        // =========================================================== BASE
        if (wantBase)
        {
            const bid = id + "base";
            var solids = [];

            solids = append(solids, cylZ(context, bid, "hub", 0, 0, 0, armRootH, hubR));

            // A BUTTRESS, not a pedestal. The old one was a full cylinder to
            // clutchZ, which left 18 mm of bare tube standing above the hub with
            // its +Y half carved off by the tongue slot - the stray "part of an
            // extruded cylinder". It was redundant anyway: the ear disc reaches
            // down to clutchZ - clutchOD/2, well inside the hub. All that is
            // actually wanted is material under the ear, confined to the ear's
            // own Y band so the tongue never meets it.
            solids = append(solids, block(context, bid, "butt",
                        [-clutchOD / 2.0 * 0.8, -(tongueT / 2.0 + earT), 0],
                        [clutchOD / 2.0 * 0.8, -tongueT / 2.0, clutchZ]));

            // ---- magnet carrier: cross arms, or a round disc ----------------
            // Everything above and below this branch is shared, so the clutch
            // interface is identical and one yaw post fits either base.
            //
            // ROUND is for thick plate. On 3/8 in steel the magnets deliver full
            // catalogue pull instead of the ~0.45 a saturating roof sheet allows,
            // so the lever arm no longer has to make up the difference and the
            // base can be a fraction of the diameter.
            if (isRound)
            {
                var disc = cylZ(context, bid, "disc", 0, 0, 0, roundT, roundD / 2.0);
                var rpockets = [];
                for (var k = 0; k < roundMags; k += 1)
                {
                    const ma = k * 360.0 / roundMags;
                    rpockets = append(rpockets, cylZ(context, bid, "rmag" ~ toString(k),
                                roundMagR * cos(ma * degree), roundMagR * sin(ma * degree),
                                -1.0, pocketTop, magD / 2.0));
                }
                cutWith(context, bid, "rmagcut", disc, rpockets);
                solids = append(solids, disc);
            }
            else
            {

            // One arm, built pointing +X, then patterned. Cutting the magnet
            // pockets BEFORE the pattern means they come along for free.
            const tipR = magR + padHalfR;
            var armParts = [];
            armParts = append(armParts, block(context, bid, "armbeam",
                        [0, -armW / 2.0, 0], [tipR, armW / 2.0, armRootH]));
            armParts = append(armParts, block(context, bid, "armpad",
                        [magR - padHalfR, -padHalfT, 0], [tipR, padHalfT, armTipH]));
            var arm = unionInto(context, bid, "armu", armParts[0], [armParts[1]]);

            // Taper: slice the beam down from armRootH at the hub to armTipH
            // where the magnet pad starts. One rotated block does it.
            //
            // Three things this has to get right, and all three bit:
            //   the wedge's underside must START at armRootH, not at 0, or it
            //   engulfs the arm at the hub end and the cut returns an empty
            //   body ("Boolean operation failed to return a valid part");
            //   the axis is +Y so the face falls as x grows, not -Y which
            //   tapers it backwards; and it must stop at the pad, or it shaves
            //   the material covering the magnet pockets.
            if (armTipH < armRootH - 0.01)
            {
                const run = (magR - padHalfR) - hubR;
                if (run > 1.0)
                {
                    const ang = atan2(armRootH - armTipH, run) / degree;
                    // Runs to the TIP, not to the pad. Stopping at the pad left
                    // the last 19 mm of beam at full height sitting on a 16 mm
                    // pad - the block on the end of each foot.
                    // Wider than the PAD, not than the beam. At armW it missed a
                    // 1.45 mm strip down each side of the pad and left a lip up
                    // to 3.7 mm tall at the tip.
                    const wedgeY = max(armW, padHalfT) + 5.0;
                    const wedge = block(context, bid, "taper",
                            [0, -wedgeY, armRootH],
                            [tipR + 2.0, wedgeY, armRootH + 3 * armRootH]);
                    opTransform(context, bid + "taperrot", {
                                "bodies" : wedge,
                                "transform" : rotationAround(
                                        line(p3(hubR, 0, armRootH), vector(0, 1, 0)), ang * degree)
                            });
                    cutWith(context, bid, "tapercut", arm, [wedge]);
                }
            }

            // Magnet pockets: open at z = 0 so the magnet sits DEAD FLUSH on
            // the roof. A 0.4 mm print membrane under each one would cost
            // about a tenth of the pull, and there is not a tenth to spare.
            var pockets = [];
            for (var k = 0; k < nPer; k += 1)
            {
                const off = (k - (nPer - 1) / 2.0) * magPitch;
                pockets = append(pockets, cylZ(context, bid, "mag" ~ toString(k),
                            magR, off, -1.0, pocketTop, magD / 2.0));
                // Default 0. A hole through the top face drains straight onto a
                // bare NdFeB magnet and a steel yoke and pools there. The magnets
                // are glued in; this is not a serviceable joint.
                if (pushD > 0.01)
                    pockets = append(pockets, cylZ(context, bid, "push" ~ toString(k),
                                magR, off, pocketTop - 0.5, armTipH + 1.0, pushD / 2.0));
            }
            cutWith(context, bid, "magcut", arm, pockets);

            var armAll = [arm];
            if (nArms > 1)
            {
                var xf = [];
                var nm = [];
                for (var k = 1; k < nArms; k += 1)
                {
                    xf = append(xf, rotationAround(line(p3(0, 0, 0), vector(0, 0, 1)),
                                (k * 360.0 / nArms) * degree));
                    nm = append(nm, "arm" ~ toString(k));
                }
                opPattern(context, bid + "armpat", {
                            "entities" : arm,
                            "transforms" : xf,
                            "instanceNames" : nm
                        });
                armAll = append(armAll, qCreatedBy(bid + "armpat", EntityType.BODY));
            }
            for (var a in armAll)
                solids = append(solids, a);

            }   // end cross-arm branch

            // Gussets behind the ear. It is a cantilevered plate once the
            // clutch went single-sided, and a side load bends it about its
            // foot. TWO of them, FLANKING the nut pocket rather than covering
            // it - the channel between them is how the nyloc gets in.
            if (gussetD > 0.01)
            {
                const gi = nutAF / 2.0 + 2.0;
                const gTop = clutchZ + clutchOD / 2.0 * 0.55;
                const gy0 = -(tongueT / 2.0 + earT) - gussetD;
                const gy1 = -(tongueT / 2.0 + earT) + 2.0;      // bite into the ear
                const gA = block(context, bid, "gusA", [gi, gy0, armRootH - 3.0],
                        [gi + gussetT, gy1, gTop]);
                const gB = block(context, bid, "gusB", [-gi - gussetT, gy0, armRootH - 3.0],
                        [-gi, gy1, gTop]);
                // Diagonal off the top outer corner, cut on the gussets ALONE so
                // the plane cannot reach the ear behind them.
                const ang = atan2(gTop - armRootH, gussetD) / degree;
                // The block's UNDERSIDE has to start at the pivot's height, not
                // at z = 0. Built from 0 it lands ~150 mm low once rotated and
                // swallows both gussets whole. It is also bounded at the ear
                // face so it can only ever reach the gussets.
                const gtrim = block(context, bid, "gtrim",
                        [-2.0 * clutchOD, -(tongueT / 2.0 + earT) - 4.0 * clutchOD, gTop],
                        [2.0 * clutchOD, -(tongueT / 2.0 + earT), gTop + 4.0 * clutchOD]);
                opTransform(context, bid + "gtrimrot", {
                            "bodies" : gtrim,
                            "transform" : rotationAround(
                                    line(p3(0, -(tongueT / 2.0 + earT), gTop), vector(1, 0, 0)),
                                    ang * degree)
                        });
                cutWith(context, bid, "gcut", qUnion([gA, gB]), [gtrim]);
                solids = append(solids, gA);
                solids = append(solids, gB);
            }

            // ORDER MATTERS HERE. The tongue is a disc that swings, and the
            // hub and pedestal fill the entire gap between the ears - without
            // a slot the yaw post cannot be fitted at all, let alone rotated.
            // But the slot has to be wider than the tongue's ridge extent,
            // and the ears' tooth faces start INSIDE that width. So the slot
            // is cut while the base is still hub + pedestal + arms, and the
            // ears are unioned on afterwards where nothing can undercut them.
            var shell = unionInto(context, bid, "u1", solids[0],
                    subArray(solids, 1, size(solids)));
            // Only needed if the tongue actually dips into the hub - and it must
            // NOT. A disc in a cylindrical pocket can only go in along the clutch
            // axis, and the hub walls that path off past the end of any slot we
            // could cut without sawing the +Y arm in half. clutchZ is set so the
            // tongue clears the hub entirely; the slot stays as a guarantee if
            // someone lowers it again.
            if (clutchZ - clutchOD / 2.0 < armRootH)
                cutWith(context, bid, "slotcut", shell,
                        [cylY(context, bid, "tongueslot", 0, clutchZ,
                                -tongueT / 2.0, tongueT / 2.0 + earT + 10.0,
                                clutchOD / 2.0 + jointClear)]);

            // Tether eye gets a raised lug. A bare hole through one arm and none
            // of the others reads as a mistake; a boss reads as a fitting.
            // Cross base only. The round base goes on thick plate where the
            // magnets hold about 2.2x as hard, so the tether is not carrying the
            // rating the way it does on a roof.
            const tethR = hubR + (magR - hubR) * 0.35;
            if (tetherD > 0.01 && !isRound)
                shell = unionInto(context, bid, "tethboss", shell,
                        [cylZ(context, bid, "tethb", tethR, 0, 0, armRootH,
                                tetherD / 2.0 + 4.5)]);

            // ONE ear, not two. A face spline can only engage along its axis,
            // and the tongue's ridges are wider than the gap between two rigid
            // ears - a double-shear clutch like that has NO assembly sequence
            // unless one side is a separate bolted-on part. Single-sided costs
            // nothing here: the teeth carry the moment across a full annulus
            // and the M6 only has to clamp, which it does at ~10x the margin.
            const g = tongueT / 2.0;
            const ear = cylY(context, bid, "ear", 0, clutchZ, -g - earT, -g, clutchOD / 2.0);
            var base = unionInto(context, bid, "u2", shell, [ear]);

            // Teeth into both ear inner faces. Ears get spin 0 and zero extra
            // depth; the tongue gets half a pitch and the clearance. That is
            // what makes the two zigzags mesh.
            const ringEar = vToothRing(context, bid, "tA", nTeeth, clutchOD, clutchID, cDepth, 0);
            placeRing(context, bid, "tA", ringEar, -90, p3(0, -g, clutchZ));

            var cuts = [ringEar];
            // Relief at the centre of each face: inside the ID the grooves
            // overlap and chew the surface, so cut that region away cleanly.
            cuts = append(cuts, cylY(context, bid, "rel", 0, clutchZ,
                        -g - cDepth - 0.3, -g + 0.1, clutchID / 2.0));

            // Axis bolt, and either a captive nyloc pocket or a plain hole.
            const holeR = (sleeveD > 0.01 ? sleeveD : boltD) / 2.0;
            cuts = append(cuts, cylY(context, bid, "axis", 0, clutchZ,
                        -g - earT - 2.0, g + 2.0, holeR));
            if (definition.clutchNut == NutStyle.CAPTIVE)
            {
                const hx = hexPrismZ(context, bid, "nut", 0, 0, -nutDepth / 2.0, nutDepth / 2.0, nutAF);
                opTransform(context, bid + "nutrot", {
                            "bodies" : hx,
                            "transform" : rotationAround(line(p3(0, 0, 0), vector(1, 0, 0)), 90 * degree)
                        });
                opTransform(context, bid + "nutmv", {
                            "bodies" : hx,
                            "transform" : transform(p3(0, -g - earT + nutDepth / 2.0 - 0.01, clutchZ))
                        });   // outboard face of the single ear
                cuts = append(cuts, hx);
            }

            // The nyloc pocket opens on the ear's outboard face, but the
            // pedestal wraps past it and walls in the lower half of the hex.
            // Open a channel straight out so the nut can be dropped in.
            if (definition.clutchNut == NutStyle.CAPTIVE)
            {
                cuts = append(cuts, block(context, bid, "nutaccess",
                            [-nutAF / 2.0 - 0.3, -(tongueT / 2.0 + earT) - clutchOD,
                                clutchZ - nutAF / 2.0 - 0.3],
                            [nutAF / 2.0 + 0.3, -(tongueT / 2.0 + earT) + 0.1,
                                clutchZ + nutAF / 2.0 + 0.3]));
            }

            if (tetherD > 0.01 && !isRound)
                cuts = append(cuts, cylZ(context, bid, "teth",
                            tethR, 0, -1.0, armRootH + 1.0, tetherD / 2.0));

            cutWith(context, bid, "cuts", base, cuts);

            // Break the ground edge. Relieves elephant-foot on the first layer
            // and stops the base reading as a slab with a sawn edge.
            ringAt(context, bid, "ground", base, 0, edgeBreak, false, 60.0);
            // Arm and hub tops. Falls back per-edge, and silently does nothing
            // if the taper leaves nothing horizontal to take it.
            ringAt(context, bid, "deck", base, armRootH, edgeBreak, false, 60.0);
            if (isRound)
                ringAt(context, bid, "discblend", base, roundT, blendR, true, 60.0);

            setProperty(context, { "entities" : base, "propertyType" : PropertyType.NAME, "value" : "Base" });
        }

        // ====================================================== YAW POST
        if (wantPost)
        {
            const pid = id + "post";
            var solids = [];
            const g = tongueT / 2.0;

            // Blank runs to g + cDepth, NOT to g. Both halves are the same
            // zigzag; the tongue's ridges have to cross the ear's face plane
            // to fill the ear's valleys. Cutting both faces at y = +/-g would
            // leave two zigzags on opposite sides of one plane, touching at
            // points and carrying nothing.
            // Asymmetric: ridges reach past the ear's face on -Y, plain on +Y.
            // The 12 mm core and the neck stay centred, so the mast axis is
            // still dead on the base centreline.
            const tRidge = g + cDepth;
            solids = append(solids, cylY(context, pid, "tongue", 0, clutchZ, -tRidge, g, clutchOD / 2.0));

            // The column CANNOT be a fat cylinder off the tongue. The ear discs
            // reach up to clutchZ + clutchOD/2 and sit at |y| = tongueT/2 and
            // out, so anything wider than the tongue would pass straight
            // through them. Below the ear tops it is a blade no thicker than
            // the tongue; only above does it flare to carry the azimuth ring.
            const earTop = clutchZ + clutchOD / 2.0;
            // THE FLANGE MUST CLEAR THE TONGUE'S APEX, and must be DERIVED from
            // earTop, not hard-coded.
            //
            // The tongue is a disc lying in XZ, so its topmost point is a single
            // LINE at z = clutchZ + clutchOD/2. The flange's underside was a
            // hard-coded azZ - 9. When clutchOD went 50 -> 56 those two numbers
            // became equal, so the tongue met the flange along that one line and
            // nowhere else - a tangential line contact between two solids in a
            // union, which is non-manifold by definition. Onshape's only symptom
            // was "Boolean operation would result in non-manifold body".
            const flareZ = earTop + 2.0;

            // The neck runs all the way to azZ now, so the flange has real
            // overlap to sit on instead of grazing the tongue.
            const bladeX = clutchOD / 2.0 * 0.8;
            solids = append(solids, block(context, pid, "neck",
                        [-bladeX, -tongueT / 2.0, clutchZ], [bladeX, tongueT / 2.0, azZ]));
            // No column here any more. It was r = azOD/2 spanning flareZ..azZ,
            // which is EXACTLY the radius and an overlapping z range of the
            // azimuth boss below - two coaxial cylinders with coincident
            // surfaces, which is precisely the "non-manifold body" case. It was
            // also redundant: that space is entirely inside the clamp flange,
            // and the neck already overlaps the flange on its own.
            // Clamp flange. The azimuth used to be held by one bolt down the
            // mast axis - the same axis the coax occupies and the N bulkhead
            // caps. That made azimuth adjustment impossible without stripping
            // the whole RF path first. The clamp is now a bolt circle OUTSIDE
            // the tooth ring, reachable with the antenna in place.
            solids = append(solids, cylZ(context, pid, "azflange", 0, 0,
                        flareZ, azZ, azFlangeR));
            // No boss. It existed only to carry the azimuth ridges above the
            // flange; with the teeth gone the flange IS the mating face, a plain
            // annulus at azZ, which is also the face this part prints on.

            var post = unionInto(context, pid, "u", solids[0], subArray(solids, 1, size(solids)));

            // Tongue teeth: half a pitch of spin, and cut deeper by the flank
            // clearance so it drops into the ears without binding.
            const rA = vToothRing(context, pid, "tA", nTeeth, clutchOD, clutchID, cDepth + toothClear, cStep / 2.0);
            placeRing(context, pid, "tA", rA, 90, p3(0, -tRidge, clutchZ));

            var cuts = [rA];
            // Inside the ID the grooves overlap and chew the surface. Take the
            // ridges off entirely there and leave the core flat at +/-g.
            cuts = append(cuts, cylY(context, pid, "rel", 0, clutchZ,
                        -tRidge - 1.0, -g, clutchID / 2.0));

            const holeR = (sleeveD > 0.01 ? sleeveD : boltD) / 2.0;
            cuts = append(cuts, cylY(context, pid, "axis", 0, clutchZ,
                        -tRidge - 2.0, g + 2.0, holeR));

            // No azimuth ridges - see the head. The flange's top face is the
            // mating face now, flat, and it is also the face this part prints on.
            //
            // ONE CLEARANCE HOLE PER INDEX POSITION, not one per bolt. With the
            // teeth gone this flange is the ONLY thing that indexes the azimuth,
            // so the holes are no longer a convenience - they are the feature.
            // With 3 holes at 120 deg against 3 inserts at 120 deg, the bolts
            // only lined up when the head was turned a whole 120 deg, so 21 of
            // the 24 positions could not be bolted at all.
            // The head keeps its 3 inserts (they are the expensive side). Three
            // of these holes are used at a time, always 120 deg apart, and the
            // rest are open - which also drains the joint.
            // azSteps must divide by azBoltN or the three bolts cannot all land
            // on holes at once; guarded below.
            for (var k = 0; k < azN; k += 1)
            {
                const a = k * 360.0 / azN;
                cuts = append(cuts, cylZ(context, pid, "azcl" ~ toString(k),
                            azBoltR * cos(a * degree), azBoltR * sin(a * degree),
                            azZ - 10.0, azZ + 0.1, azBoltD / 2.0));
            }

            cutWith(context, pid, "cuts", post, cuts);

            // Only the flare's shoulder. The azimuth face above it is toothed
            // and must not be touched.
            ringAt(context, pid, "flareblend", post, flareZ, blendR, true, 40.0);

            setProperty(context, { "entities" : post, "propertyType" : PropertyType.NAME, "value" : "Yaw post" });
        }

        // ============================================================ HEAD
        if (wantHead)
        {
            const hid = id + "head";
            var solids = [];

            // FLAT BOTTOM, and everything below follows from it. Nothing on this
            // part dips under azZ, so it prints straight onto the bed with no
            // support anywhere. That is why the azimuth teeth are GROOVES here
            // and RIDGES on the post: ridges would hang below this plane and put
            // the whole part back on supports. A printed test of the old version
            // failed outright, so this is not a preference.
            solids = append(solids, cylZ(context, hid, "azflange", 0, 0,
                        azZ, azZ + azFlangeH, azFlangeR));
            solids = append(solids, cylZ(context, hid, "mast", 0, 0, azZ, nPadZ, mastOD / 2.0));
            solids = append(solids, cylZ(context, hid, "npad", 0, 0, nPadZ - nPadT - 4.0, nPadZ, nPadR));

            // Strut out to the interface pad, and the pad itself. Both are
            // built flat at the origin and then rotated into the panel plane,
            // which keeps the trigonometry in one place.
            const padXf = toWorld(coordSystem(p3(armReach, 0, padZ),
                        vector(cos(panelTilt * degree), 0.0, -sin(panelTilt * degree)),
                        vector(sin(panelTilt * degree), 0.0, cos(panelTilt * degree))));

            // THE PAD IS A TRAPEZOID, NOT A RECTANGLE, AND THE TAPER IS STRUCTURAL
            // TO THE PRINT. The pad is wider than the arm so the bolts can be
            // reached from underneath (see the holes below), which leaves its ears
            // hanging over nothing. That is fine everywhere except at the pad's
            // LOWEST point - the back face's down-plane corner - because that is
            // where the ear's first layer goes down. Rectangular, that first layer
            // is a 16 mm line extruded into open air 6 mm above the bed on each
            // side, and it droops. Everything above it is a 45 deg plane and
            // self-supporting; only the seed is the problem.
            // So the pad starts narrower than the arm and widens going up-plane,
            // no faster than MC_PAD_OVERHANG allows. Each layer then lands on the
            // one below and no support is needed anywhere on this part.
            const padNarrow = max(strutW, flareW) / 2.0 - 2.0;
            const taperK = sin(panelTilt * degree) * tan(MC_PAD_OVERHANG * degree);
            const taperRun = (ifW / 2.0 - padNarrow) / taperK;
            const lxFull = ifL / 2.0 - taperRun;
            if (lxFull < -ifL / 2.0)
                reportFeatureWarning(context, id,
                    "The pad is too short to reach its full width at a printable "
                    ~ "taper. Lengthen it or narrow it.");
            var pad = block(context, hid, "pad",
                    [-ifL / 2.0, -ifW / 2.0, -ifPadT], [ifL / 2.0, ifW / 2.0, 0]);
            // Same construction as the arm's flare trims, in the pad's own frame:
            // a slab laid against the full-width edge, pivoted about the point
            // where full width is reached. Written per side rather than looped so
            // tools/dims.py can still read the block extents.
            const pAng = atan2(ifW / 2.0 - padNarrow, taperRun) / degree;
            const pBig = 6.0 * ifL;
            const ptA = block(context, hid, "ptrimA",
                    [lxFull - pBig, ifW / 2.0, -2.0 * ifPadT],
                    [lxFull + pBig, ifW / 2.0 + pBig, ifPadT]);
            opTransform(context, hid + "ptrimArot", {
                        "bodies" : ptA,
                        "transform" : rotationAround(
                                line(p3(lxFull, ifW / 2.0, 0), vector(0, 0, 1)),
                                (-pAng) * degree)
                    });
            const ptB = block(context, hid, "ptrimB",
                    [lxFull - pBig, -ifW / 2.0 - pBig, -2.0 * ifPadT],
                    [lxFull + pBig, -ifW / 2.0, ifPadT]);
            opTransform(context, hid + "ptrimBrot", {
                        "bodies" : ptB,
                        "transform" : rotationAround(
                                line(p3(lxFull, -ifW / 2.0, 0), vector(0, 0, 1)),
                                pAng * degree)
                    });
            cutWith(context, hid, "padcut", pad, [ptA, ptB]);
            opTransform(context, hid + "padmv", { "bodies" : pad, "transform" : padXf });
            solids = append(solids, pad);

            // Strut: a beam from the mast to the back of the pad.
            // Runs PAST the pad centre. Ending square at armReach left a wedge
            // of air under the mount - the pad's back face only meets the
            // strut's underside further out. The back trim faces it flush.
            // Underside sits on azZ, not padZ - strutH/2. That is what makes the
            // whole bottom one plane; the arm becomes a deep wedge that faceTrim
            // shapes into the pad, and it is stiffer than the old beam for free.
            //
            // It must STOP SHORT of where the pad plane exits through that
            // underside, or the arm tapers to a literal knife edge and the
            // boolean returns a non-manifold body. The plane crosses z = azZ at
            // armReach + (padZ - azZ); ending 5 mm inside that leaves a 5 mm end
            // face instead of a zero-thickness feather.
            const strutEnd = armReach + (padZ - azZ) - 5.0;

            // SHORTEN THE ARM FROM THE TOP, INBOARD.
            // The arm was 26x overbuilt in bending and its depth looked
            // untouchable, because the coax channel held the top up over the
            // inboard half. But the channel's height is a CHOICE - it is cut
            // down from the top, so lowering the top lowers the channel with
            // it. Only the outboard end is really pinned, by the pad's back
            // plane, which the arm has to meet at `padFull` or the up-plane
            // interface bolt loses its backing.
            // So: the arm runs `strutDrop` lower from the mast out, then climbs
            // back over `strutRamp` to full height before the pad needs it.
            // The climb is an UPWARD-facing slope, so it costs nothing to print.
            const armTop = padZ + strutH / 2.0;
            const lowTop = armTop - strutDrop;
            const padFull = armReach + padZ - 2.0 * ifPadT * cos(panelTilt * degree)
                    - armTop;
            const rampX = padFull - strutRamp;
            const strut = block(context, hid, "strut",
                    [0, -strutW / 2.0, azZ],
                    [strutEnd, strutW / 2.0, lowTop]);
            solids = append(solids, strut);

            // The strut is 24 wide, not 32. At 32 the arm was 26x overbuilt in
            // bending at 90 mph, and none of its DEPTH is surplus - the top is
            // held up by the coax channel out to x=67 and by the pad's back
            // plane from x=80 on, and the bottom is the print bed. The width was
            // the only free dimension. 24 keeps 4 mm of wall either side of the
            // 16 mm channel and still leaves a safety factor of 16.
            //
            // KNEE BRACE. The arm is a cantilever and its root is the most
            // loaded spot on the whole mount - about 2.3 Nm at 60 mph. The
            // mast carries on upward anyway, so tying the two together costs
            // almost nothing and roughly doubles the section depth at the root.
            if (kneeLen > 0.01)
            {
                const kneeTop = lowTop + kneeLen;
                const kneeOut = mastOD / 2.0 + kneeLen * 1.8;
                // Full strut width. The coax channel then cuts UP THROUGH it and
                // splits it into two webs flanking an open channel. The old
                // brace was narrower than the channel, so it sat entirely
                // inside it: it roofed the cable run end to end, and the channel
                // cut severed its whole footprint on the strut, leaving it
                // attached to the mast alone and carrying nothing.
                var knee = block(context, hid, "knee",
                        [0, -strutW / 2.0, lowTop],
                        [kneeOut, strutW / 2.0, kneeTop]);
                // Diagonal cut, on the brace ALONE so it cannot touch the mast.
                // Block underside starts at the pivot, or the plane lands elsewhere.
                const kAng = atan2(kneeLen, kneeOut - mastOD / 2.0) / degree;
                const ktrim = block(context, hid, "ktrim",
                        [kneeOut - 6.0 * armReach, -strutW, lowTop],
                        [kneeOut, strutW, lowTop + 6.0 * armReach]);
                opTransform(context, hid + "ktrimrot", {
                            "bodies" : ktrim,
                            "transform" : rotationAround(
                                    line(p3(kneeOut, 0, lowTop), vector(0, 1, 0)),
                                    kAng * degree)
                        });
                cutWith(context, hid, "kneecut", knee, [ktrim]);
                solids = append(solids, knee);
            }

            // Arm flares into the pad. A 22 mm strut meeting a 60 mm plate is
            // both a stress riser and the thing that makes the head look
            // unfinished; the flare fixes both.
            if (flareW > strutW + 1.0)
            {
                const fStart = armReach * 0.52;
                var flare = block(context, hid, "flare",
                        [fStart, -flareW / 2.0, azZ],
                        [strutEnd, flareW / 2.0, lowTop]);

                // SHOULDER - the full-height band, and only where something
                // actually needs it. It is ADDED, never cut: a chop big enough
                // to take this band off the arm also reaches the mast, the N pad
                // and the pad's own tab, and clipping the pad's back corner
                // leaves exactly the sliver that makes a boolean non-manifold.
                // Unioned into the flare BEFORE the flare's Y-trim, so it picks
                // up the same taper for free - full width here would overhang
                // the flare by 9 mm a side with nothing under it.
                if (strutDrop > 0.01)
                {
                    var shoulder = block(context, hid, "shoulder",
                            [rampX, -flareW / 2.0, lowTop],
                            [strutEnd, flareW / 2.0, armTop]);
                    // Ramp its inboard end instead of stepping: the top climbs
                    // from lowTop at rampX to armTop at padFull. An upward-facing
                    // slope costs nothing to print and takes the stress riser out
                    // of the top fibre.
                    const rampA = atan2(strutDrop, strutRamp) / degree;
                    const rBig = 6.0 * armReach;
                    // rtrim occupies the space ABOVE the ramp, because what has
                    // to come off the shoulder is everything higher than the
                    // desired top. Built below it instead - which is the natural
                    // way to write "a wedge under the ramp" - it keeps the exact
                    // complement: a triangle full height at rampX and ZERO from
                    // padFull out, which is the only place the shoulder exists
                    // for. That shipped, and it left the pad floating over a
                    // 14 mm gap with a fin standing next to it.
                    const rtrim = block(context, hid, "rtrim",
                            [rampX, -flareW, lowTop],
                            [rampX + rBig, flareW, lowTop + rBig]);
                    opTransform(context, hid + "rtrimrot", {
                                "bodies" : rtrim,
                                "transform" : rotationAround(
                                        line(p3(rampX, 0, lowTop), vector(0, 1, 0)),
                                        (-rampA) * degree)
                            });
                    cutWith(context, hid, "shouldercut", shoulder, [rtrim]);
                    flare = unionInto(context, hid, "shoulderu", flare, [shoulder]);
                }
                const fAng = atan2((flareW - strutW) / 2.0, armReach - fStart) / degree;
                const fBig = 6.0 * armReach;
                // Written out per side rather than looped: a loop variable makes
                // the block name dynamic, which puts it beyond tools/dims.py and
                // hides it from the dimension check.
                const ftA = block(context, hid, "ftrimA",
                        [fStart - fBig, strutW / 2.0, padZ - strutH],
                        [armReach + fBig, strutW / 2.0 + fBig, padZ + strutH]);
                opTransform(context, hid + "ftrimArot", {
                            "bodies" : ftA,
                            "transform" : rotationAround(
                                    line(p3(fStart, strutW / 2.0, 0), vector(0, 0, 1)),
                                    fAng * degree)
                        });
                const ftB = block(context, hid, "ftrimB",
                        [fStart - fBig, -strutW / 2.0 - fBig, padZ - strutH],
                        [armReach + fBig, -strutW / 2.0, padZ + strutH]);
                opTransform(context, hid + "ftrimBrot", {
                            "bodies" : ftB,
                            "transform" : rotationAround(
                                    line(p3(fStart, -strutW / 2.0, 0), vector(0, 0, 1)),
                                    (-fAng) * degree)
                        });
                cutWith(context, hid, "flarecut", flare, [ftA, ftB]);
                solids = append(solids, flare);
            }

            var head = unionInto(context, hid, "u", solids[0], subArray(solids, 1, size(solids)));

            // FACE THE PAD OFF. The strut runs out to the pad centre while the
            // pad lies at 45 degrees across it, so a wedge of strut stands
            // about 9 mm proud of the mating face - the top plate would rock on
            // it. Everything on the device side of that plane gets cut away.
            const faceTrim = block(context, hid, "facetrim",
                    [-3.0 * ifL, -3.0 * ifW, 0.0], [3.0 * ifL, 3.0 * ifW, 4.0 * ifL]);
            opTransform(context, hid + "facetrimmv",
                    { "bodies" : faceTrim, "transform" : padXf });
            cutWith(context, hid, "facecut", head, [faceTrim]);

            // NO back trim. An earlier version faced off everything behind the
            // pad's back plane inside the pad footprint, which is not a flush
            // trim at all - it gutted the arm from x=33 outward. The pad is
            // 7 mm thick; a 26 mm beam is SUPPOSED to sit behind it. Extending
            // strutEnd past the pad and letting faceTrim cut the front is the
            // whole fix: the arm ends in a clean 45 deg face on the mount.

            // No locating rib. Four bolts locate the plate perfectly well, and
            // the rib was the one feature allowed to stand proud of the pad -
            // which meant a clearance slot in every top plate to receive it.

            // NO AZIMUTH TEETH. They were a face spline here and ridges on the
            // post, and they were carrying nothing: rotation about the mast axis
            // is the ONE direction a symmetric wind cannot load - the panel force
            // and its position vector both lie in the vertical plane through the
            // axis, so r x F has no z-component. What is left is eccentricity,
            // about 1.1 N.m at 90 mph and 3.3 bounding. Three M4s at r=30 hold
            // 12.6 N.m by friction alone on a preload already halved for creep,
            // and the 24 index holes are a positive backstop at 0.76 deg of
            // backlash. The CLUTCH teeth stay - that joint carries the full
            // 9.5 N.m overturning moment on one bolt and friction gives it SF 1.5
            // before creep, which is not enough.
            // The win is the print: these grooves were cut into the head's FIRST
            // LAYER, and the matching ridges were the post's first layer, squashed
            // flat. Both faces are now plain flat annuli.
            var cuts = [];

            // Heat-set inserts, blind from the underside. Bolts come up from
            // beneath because the arm sits over this bolt circle. No counterbore
            // anywhere: the heads stay proud underneath, where fingers reach.
            for (var k = 0; k < azBoltN; k += 1)
            {
                const a = k * 360.0 / azBoltN;
                cuts = append(cuts, cylZ(context, hid, "azins" ~ toString(k),
                            azBoltR * cos(a * degree), azBoltR * sin(a * degree),
                            azZ - 0.1, azZ + azInsDep, azInsD / 2.0));
            }

            // Mast bore: coax route only. Nothing is reached through it now.
            // Stops BELOW the hex pocket, so the bore steps down twice on the way
            // up: oe20 body clearance, then the hex that stops it rotating, then
            // the thread hole. The N bulkhead installs from underneath, and a
            // straight oe14 bore capped by the pad gave it no way in at all.
            cuts = append(cuts, cylZ(context, hid, "bore", 0, 0,
                        azZ - 1.0, nPadZ + 1.0, mastBore / 2.0));

            // N bulkhead. The pad is spot-faced from below so the nut lands on
            // a thin section while the structure around it stays thick - the
            // bulkhead's thread is short and the nut has to engage.
            // Inserts for the antenna cap. The bulkhead itself is NOT mounted
            // here - see the cap below.
            for (var k = 0; k < capBoltN; k += 1)
            {
                const ca = k * 360.0 / capBoltN;
                cuts = append(cuts, cylZ(context, hid, "capins" ~ toString(k),
                            capBoltR * cos(ca * degree), capBoltR * sin(ca * degree),
                            nPadZ - capInsDep, nPadZ + 0.1, capInsD / 2.0));
            }

            // Coax exit and an open channel along the strut, so the pigtail
            // leaves the bore and runs to the device without a closed duct.
            if (coaxW > 0.01)
            {
                // ONE FLOOR LEVEL. The outlet used to sit 3 mm below the channel
                // so the cable had room to turn out of the bore; all it did was
                // put a step in the tray for the cable to catch on.
                const chFloor = lowTop - coaxW * 0.8;
                const chEnd = armReach - coaxStop;
                // Outlet through the mast wall, kept BELOW the knee brace so the
                // brace still lands on the mast either side of it.
                // Reaches 3 mm PAST where the channel starts so the two tools
                // OVERLAP. Meeting exactly at the mast surface gave them a
                // partial shared face - their z ranges differ - which is a
                // T-junction edge in qUnion, and a non-manifold tool makes the
                // whole subtraction non-manifold.
                cuts = append(cuts, block(context, hid, "coaxexit",
                            [0, -coaxW / 2.0, chFloor],
                            [mastOD / 2.0 + 3.0, coaxW / 2.0, lowTop - 1.0]));
                // STOP SHORT OF THE PAD. Running it to armReach dead-ends the
                // cable under the interface pad, which is precisely where the
                // repeater bolts on - there is no way out. The channel is open,
                // so ending it early is all that is needed: the cable lifts out
                // and routes to wherever the node's connector actually is.
                // Runs from the mast wall outward and OPEN TO THE SKY the whole
                // way - the top is taken above the knee brace so the brace is
                // split rather than bridging over the cable.
                var ch = block(context, hid, "coaxch",
                        [mastOD / 2.0 + 1.0, -coaxW / 2.0, chFloor],
                        [chEnd, coaxW / 2.0, lowTop + kneeLen + 2.0]);
                // RAMP THE EXIT. Square-ended, the cable has to be picked out of
                // a 12.8 mm slot with pliers. Ramping the floor up to the arm's
                // top surface over the last coaxRamp means pushing the cable in
                // from the mast end walks its end out on its own.
                //
                // The trim sits BELOW the ramp line here - the OPPOSITE of the
                // arm shoulder's, which sits above. The rule is not about the
                // ramp, it is about what the body is: `ch` is a CUT TOOL, so
                // taking a wedge out of it LEAVES that material in the part.
                // Say out loud which half survives before writing one of these.
                if (coaxRamp > 0.01)
                {
                    const cAng = atan2(coaxW * 0.8, coaxRamp) / degree;
                    const cBig = 6.0 * armReach;
                    const crt = block(context, hid, "coaxramptrim",
                            [chEnd - coaxRamp, -coaxW, chFloor - cBig],
                            [chEnd - coaxRamp + cBig, coaxW, chFloor]);
                    opTransform(context, hid + "coaxramprot", {
                                "bodies" : crt,
                                "transform" : rotationAround(
                                        line(p3(chEnd - coaxRamp, 0, chFloor),
                                            vector(0, 1, 0)),
                                        (-cAng) * degree)
                            });
                    cutWith(context, hid, "coaxrampcut", ch, [crt]);
                }
                cuts = append(cuts, ch);
            }

            // COUNTERSUNK THROUGH-HOLES, IN EARS THAT OVERHANG THE ARM.
            // This is the only place on the joint a driver can reach once the
            // device is bolted on. The plate's front face is under the repeater,
            // and the pad's back face is solid arm anywhere inboard of the
            // flare - both of the obvious directions are dead ends, and both
            // shipped. So the pad is made WIDER THAN THE ARM: bolts at y = +-32
            // against a flare that is never wider than +-24, which puts the
            // whole bolt axis - head, countersink and driver - in open air
            // behind the ear. Inserts go in the plate, bolts up from underneath.
            // Shifted UP-PLANE of the pad centre, not centred on it. The pad's
            // down-plane end is given over to the taper - it is only as wide as
            // the arm down there - so the bolts have to sit where the ears have
            // reached full width.
            const bx = [-ifBoltShift - ifBoltY / 2.0, -ifBoltShift + ifBoltY / 2.0];
            const by = [-ifBoltX / 2.0, ifBoltX / 2.0];
            for (var i = 0; i < 2; i += 1)
            {
                for (var j = 0; j < 2; j += 1)
                {
                    const nm = toString(i) ~ toString(j);
                    const h = cylZ(context, hid, "ifb" ~ nm, bx[i], by[j],
                            -ifPadT - 1.0, 1.0, ifBoltD / 2.0);
                    opTransform(context, hid + ("ifbm" ~ nm),
                            { "bodies" : h, "transform" : padXf });
                    cuts = append(cuts, h);
                    // No countersink. These take socket head CAP screws now, and
                    // the pad's back face out here is open air - the whole point
                    // of the ears - so the head bears straight on it, the same
                    // way the azimuth clamp's heads bear on the post's flange.
                    // A 90 deg cone printed into a 45 deg face was never much of
                    // a seat anyway.
                }
            }

            cutWith(context, hid, "cuts", head, cuts);

            // The two diameter steps up the mast are the abrupt ones. Both are
            // clean horizontal circles, which is exactly the case a positional
            // ring query handles reliably.
            ringAt(context, hid, "skirtblend", head, azZ + azFlangeH, blendR, true, 60.0);
            ringAt(context, hid, "npadblend", head, nPadZ - nPadT - 4.0, blendR, true, 60.0);
            ringAt(context, hid, "npadtop", head, nPadZ, edgeBreak, false, 60.0);

            setProperty(context, { "entities" : head, "propertyType" : PropertyType.NAME, "value" : "Head" });

            // ------------------------------------------------ ANTENNA CAP
            // The bulkhead installs from underneath, and once the head is built
            // the mast bore is blind at both ends - there is no sequence that
            // gets a connector in. So the panel it mounts to is a SEPARATE
            // part: fit the bulkhead and the pigtail to the cap on the bench,
            // with both hands and both sides reachable, then lower it on,
            // feeding the coax down the bore, and run three bolts.
            const cz1 = nPadZ + capT;
            var cap = cylZ(context, hid, "cap", 0, 0, nPadZ, cz1, nPadR);
            var capCuts = [];

            // D-hole: round with two flats, "mounting hole B". The flats are
            // what stop the connector turning when the antenna is screwed on.
            var cdh = cylZ(context, hid, "capdh", 0, 0, nPadZ - 1.0, cz1 + 1.0, nHole / 2.0);
            if (nFlat > 0.01 && nFlat < nHole)
            {
                cutWith(context, hid, "capflats", cdh, [
                            block(context, hid, "cfa", [-nHole, nFlat / 2.0, nPadZ - 2.0],
                                    [nHole, nHole, cz1 + 2.0]),
                            block(context, hid, "cfb", [-nHole, -nHole, nPadZ - 2.0],
                                    [nHole, -nFlat / 2.0, cz1 + 2.0])
                        ]);
            }
            capCuts = append(capCuts, cdh);

            // Relief underneath so the section at the hole is nPadT, not capT -
            // the nut has to reach thread past it.
            if (capT > nPadT + 0.01)
                capCuts = append(capCuts, cylZ(context, hid, "caprelief", 0, 0,
                            nPadZ - 1.0, nPadZ + (capT - nPadT), nHole / 2.0 + 5.0));
            if (nSpot > 0.01)
                capCuts = append(capCuts, cylZ(context, hid, "capspot", 0, 0,
                            cz1 - nSpot, cz1 + 1.0, max(nHole / 2.0 + 3.0, 9.0)));

            for (var k = 0; k < capBoltN; k += 1)
            {
                const cb = k * 360.0 / capBoltN;
                capCuts = append(capCuts, cylZ(context, hid, "capb" ~ toString(k),
                            capBoltR * cos(cb * degree), capBoltR * sin(cb * degree),
                            nPadZ - 1.0, cz1 + 1.0, capBoltD / 2.0));
            }
            cutWith(context, hid, "capcuts", cap, capCuts);
            ringAt(context, hid, "captop", cap, cz1, edgeBreak, false, 60.0);
            setProperty(context, { "entities" : cap, "propertyType" : PropertyType.NAME,
                        "value" : "Antenna cap" });

            // Preview only: swing the head to a tilt and azimuth so clearances
            // can be eyeballed. Print at zero.
            if (definition.azPreview != 0 * degree)
                opTransform(context, hid + "azprev", {
                            "bodies" : qUnion([head, cap]),
                            "transform" : rotationAround(line(p3(0, 0, 0), vector(0, 0, 1)), definition.azPreview)
                        });
            if (definition.tiltPreview != 0 * degree)
                opTransform(context, hid + "tiltprev", {
                            "bodies" : qUnion([head, cap]),
                            "transform" : rotationAround(line(p3(0, 0, clutchZ), vector(0, -1, 0)), definition.tiltPreview)
                        });
        }
    }, {
            "parts" : MountParts.ALL,
            "baseStyle" : BaseStyle.CROSS,
            "roundD" : 152.4 * millimeter,
            "roundT" : 12.0 * millimeter,
            "roundMagR" : 60.0 * millimeter,
            "roundMags" : 8,
            "magArms" : 4,
            "magPerArm" : 2,
            "magR" : 120.0 * millimeter,
            "magPitch" : 32.0 * millimeter,
            "magD" : 12.9 * millimeter,
            "magT" : 3.175 * millimeter,
            "yokeT" : 3.0 * millimeter,
            "magWall" : 3.0 * millimeter,
            "pushD" : 0.0 * millimeter,
            "armW" : 24.0 * millimeter,
            "armRootH" : 30.0 * millimeter,
            "armTipH" : 16.0 * millimeter,
            "hubR" : 38.0 * millimeter,
            "tetherD" : 7.0 * millimeter,
            "clutchZ" : 54.0 * millimeter,
            "clutchOD" : 42.0 * millimeter,
            "clutchID" : 28.0 * millimeter,
            "clutchTeeth" : 36,
            "toothClear" : 0.15 * millimeter,
            "tongueT" : 12.0 * millimeter,
            "jointClear" : 0.35 * millimeter,
            "earT" : 12.0 * millimeter,
            "gussetD" : 14.0 * millimeter,
            "gussetT" : 8.0 * millimeter,
            "tiltMax" : 30.0 * degree,
            "edgeBreak" : 0.8 * millimeter,
            "blendR" : 3.0 * millimeter,
            "kneeLen" : 0.0 * millimeter,
            "flareW" : 48.0 * millimeter,
            "boltD" : 6.2 * millimeter,
            "sleeveD" : 0.0 * millimeter,
            "clutchNut" : NutStyle.CAPTIVE,
            "nutAF" : 10.3 * millimeter,
            "nutDepth" : 4.0 * millimeter,
            "azZ" : 85.0 * millimeter,

            "azSteps" : 24,

            "azFlangeR" : 36.0 * millimeter,
            "azFlangeH" : 12.0 * millimeter,
            "azBoltR" : 30.0 * millimeter,
            "azBoltN" : 3,
            "azBoltD" : 4.4 * millimeter,
            "azInsD" : 5.8 * millimeter,
            "azInsDep" : 8.0 * millimeter,
            "mastOD" : 40.0 * millimeter,
            "mastBore" : 20.0 * millimeter,
            "nPadZ" : 145.0 * millimeter,
            "nPadR" : 22.0 * millimeter,
            "nPadT" : 6.0 * millimeter,
            "nHole" : 16.6 * millimeter,
            "nFlat" : 14.0 * millimeter,
            "nThread" : 19.8 * millimeter,
            "capT" : 9.0 * millimeter,
            "capBoltR" : 16.0 * millimeter,
            "capBoltN" : 3,
            "capBoltD" : 3.4 * millimeter,
            "capInsD" : 5.0 * millimeter,
            "capInsDep" : 6.0 * millimeter,
            "nSpot" : 0.0 * millimeter,
            "armReach" : 105.0 * millimeter,
            "padZ" : 116.0 * millimeter,
            "panelTilt" : 45.0 * degree,
            "strutW" : 24.0 * millimeter,
            "strutDrop" : 14.0 * millimeter,
            "strutRamp" : 12.0 * millimeter,
            "strutH" : 32.0 * millimeter,
            "coaxW" : 16.0 * millimeter,
            "coaxStop" : 38.0 * millimeter,
            "coaxRamp" : 16.0 * millimeter,
            "ifW" : 80.0 * millimeter,
            "ifL" : 68.0 * millimeter,
            "ifBoltX" : 64.0 * millimeter,
            "ifBoltY" : 26.0 * millimeter,
            "ifBoltD" : 4.4 * millimeter,
            "ifHeadD" : 7.0 * millimeter,
            "ifBoltShift" : 10.0 * millimeter,
            "ifPadT" : 6.0 * millimeter,
            "tiltPreview" : 0.0 * degree,
            "azPreview" : 0.0 * degree
        });

// ============================================== FEATURE 2: THE TOP PLATE
//
// The swappable half of MC-TOP-1. Every future repeater is a new instance of
// this feature with a different device-side hole pattern - the arm never
// changes.
//
// Built flat in the panel plane, which is also how it prints:
//   +X is DOWN-plane (outboard and downhill), matching the arm pad's local X
//   +Z is the panel normal, out of the back face toward the device
//   the origin is the interface pad centre
//
// The P1-Pro hangs off two 5 mm holes 40 mm apart on a 201 mm enclosure. The
// bolts themselves barely care - about 7 N each - but the PLASTIC is the weak
// link in the sag moment, so a FULL-WIDTH lip at the bottom catches the
// enclosure's lower edge and reacts it in bearing instead. Full width, not a
// pair of tabs, because that same lip is what stops the enclosure rolling
// about the bolt axis - the two bolts sit ON that axis and cannot resist it.

const MC_PLATET_BOUNDS = { (millimeter) : [2.5, 10.0, 18.0] } as LengthBoundSpec;
const MC_PLATESIZE_BOUNDS = { (millimeter) : [40.0, 80.0, 220.0] } as LengthBoundSpec;
const MC_PADUP_BOUNDS = { (millimeter) : [-80.0, 5.0, 140.0] } as LengthBoundSpec;
const MC_HOLESP_BOUNDS = { (millimeter) : [8.0, 40.0, 160.0] } as LengthBoundSpec;
const MC_HOLED_BOUNDS = { (millimeter) : [3.0, 6.5, 14.0] } as LengthBoundSpec;
const MC_STRIPW_BOUNDS = { (millimeter) : [0.0, 24.0, 90.0] } as LengthBoundSpec;
const MC_PROUD_BOUNDS = { (millimeter) : [0.0, 0.0, 12.0] } as LengthBoundSpec;
const MC_CORNER_BOUNDS = { (millimeter) : [0.0, 5.0, 25.0] } as LengthBoundSpec;

annotation { "Feature Type Name" : "MeshCore top plate" }
export const meshcoreTopPlate = defineFeature(function(context is Context, id is Id, definition is map)
    precondition
    {
        annotation { "Group Name" : "Plate", "Collapsed By Default" : false }
        {
            annotation { "Name" : "Thickness" }
            isLength(definition.plateT, MC_PLATET_BOUNDS);

            // SQUARE, and only just bigger than the two bolt patterns it has to
            // carry. It used to be a 80 x 132 spine running the length of the
            // enclosure, which existed for a bottom lip that caught the device's
            // lower edge - see the lip note below for why that went.
            annotation { "Name" : "Size (square)" }
            isLength(definition.plateSize, MC_PLATESIZE_BOUNDS);

            annotation { "Name" : "Corner radius" }
            isLength(definition.cornerR, MC_CORNER_BOUNDS);
        }

        annotation { "Group Name" : "Device side", "Collapsed By Default" : false }
        {
            annotation { "Name" : "Bolt pair sits this far down-plane of the pad" }
            isLength(definition.padUp, MC_PADUP_BOUNDS);

            annotation { "Name" : "Bolt spacing" }
            isLength(definition.holeSpacing, MC_HOLESP_BOUNDS);

            annotation { "Name" : "Bolt clearance diameter" }
            isLength(definition.holeD, MC_HOLED_BOUNDS);

            // No bottom lip. It caught the enclosure's lower edge in bearing and
            // it is why the plate was 132 mm long; a square that size defeats
            // the point. The lip was belt-and-braces anyway - the in-plane load
            // it reacted is 9 N of gravity component on a 1.3 kg device, which
            // the two M6 bolts carry in shear without noticing. What is actually
            // lost is the ledge you could rest the enclosure on while starting
            // the bolts.

            annotation { "Name" : "Contact strip width" }
            isLength(definition.stripW, MC_STRIPW_BOUNDS);

            annotation { "Name" : "Contact strip proud - MEASURE THE RIB FIRST" }
            isLength(definition.stripProud, MC_PROUD_BOUNDS);
        }

        annotation { "Group Name" : "MC-TOP-1 interface - must match the mount", "Collapsed By Default" : true }
        {
            annotation { "Name" : "Bolt pattern across" }
            isLength(definition.ifBoltX, MC_IFBX_BOUNDS);

            annotation { "Name" : "Bolt pattern along" }
            isLength(definition.ifBoltY, MC_IFBY_BOUNDS);

            annotation { "Name" : "Bolt pattern offset up-plane" }
            isLength(definition.ifBoltShift, MC_IFBSH_BOUNDS);

            // CAP HEADS, counterbored - not countersunk. Both device holes sit
            // inside the pad's footprint, so their heads land on the face that
            // mates to the arm and have to finish flush either way; a flat
            // bottom takes a socket head cap screw where a 90 deg cone does not.
            // This is what sets the plate's thickness: an M6 cap head is 6.0
            // tall, so the plate cannot go below 8.5 mm whatever else changes.
            annotation { "Name" : "Device bolt counterbore diameter" }
            isLength(definition.holeCbD, MC_HCBD_BOUNDS);

            annotation { "Name" : "Device bolt counterbore depth" }
            isLength(definition.holeCbDep, MC_HCBDEP_BOUNDS);

            annotation { "Name" : "Heat-set insert hole diameter" }
            isLength(definition.insertD, MC_INSD_BOUNDS);

            annotation { "Name" : "Heat-set insert depth" }
            isLength(definition.insertDepth, MC_INSDEP_BOUNDS);
        }
    }
    {
        const t = definition.plateT / millimeter;
        const half = definition.plateSize / millimeter / 2.0;
        const cornerR = definition.cornerR / millimeter;
        const padUp = definition.padUp / millimeter;
        const hsp = definition.holeSpacing / millimeter;
        const hd = definition.holeD / millimeter;
        const stripW = definition.stripW / millimeter;
        const proud = definition.stripProud / millimeter;
        const bX = definition.ifBoltX / millimeter;
        const bY = definition.ifBoltY / millimeter;
        const ifBoltShift = definition.ifBoltShift / millimeter;
        const hCbD = definition.holeCbD / millimeter;
        const hCbDep = definition.holeCbDep / millimeter;
        const insD = definition.insertD / millimeter;
        const insDep = definition.insertDepth / millimeter;

        const pid = id + "plate";

        // An insert bored the full thickness breaks through onto the device
        // face. Want at least 2 mm of plate left under it.
        if (insDep + 2.0 > t + proud)
            reportFeatureWarning(context, id,
                "Only " ~ toString(t + proud - insDep) ~ " mm of plate under the heat-set "
                ~ "insert. Thicken the plate or shorten the insert - 2 mm minimum.");
        // The inserts sit outboard of the arm so the bolts can be reached from
        // underneath. That only works if the plate is wide enough to hold them.
        // The square only has to be big enough for the two bolt patterns, but
        // it does have to be big enough for BOTH of them - the MC-TOP-1 pattern
        // is the wide one at 64 mm across, and the device pair is the long one.
        if (bX / 2.0 + insD / 2.0 + 3.0 > half)
            reportFeatureWarning(context, id,
                "Interface inserts are within 3 mm of the plate edge. The plate "
                ~ "needs to be at least " ~ toString(bX + insD + 6.0) ~ " mm square.");
        if (ifBoltShift + bY / 2.0 + insD / 2.0 + 3.0 > half)
            reportFeatureWarning(context, id,
                "Interface inserts run off the up-plane edge of the plate.");
        if (abs(padUp) + hsp / 2.0 + hCbD / 2.0 + 3.0 > half)
            reportFeatureWarning(context, id,
                "Device bolt counterbores run off the edge of the plate.");
        // The counterbore eats the plate from the mating side; what is left is
        // the whole section carrying the device. This is the constraint that
        // fixes the thickness - an M6 cap head is 6.0 tall.
        if (hCbDep + 2.5 > t)
            reportFeatureWarning(context, id,
                "Only " ~ toString(t - hCbDep) ~ " mm of plate under the device bolt "
                ~ "counterbore. Thicken the plate, or use a low-head or button-head "
                ~ "screw.");
        if (hCbD < hd + 3.0)
            reportFeatureWarning(context, id,
                "Counterbore is too small to swallow the head of a bolt that fits "
                ~ "the " ~ toString(hd) ~ " mm clearance hole.");

        var solids = [];
        solids = append(solids, block(context, pid, "body",
                    [-half, -half, 0], [half, half, t]));

        if (stripW > 0.01 && proud > 0.01)
            solids = append(solids, block(context, pid, "strip",
                        [-half, -stripW / 2.0, t], [half, stripW / 2.0, t + proud]));

        var plate = unionInto(context, pid, "u", solids[0], subArray(solids, 1, size(solids)));

        var cuts = [];

        // Ø5.8 x 8, which is what an M4 insert measuring 0.227 in on its plain
        // OD actually needs. Together with the M6 cap head's counterbore this is
        // what holds the plate at 10 mm: the insert wants 8 + 2 and the head
        // wants 6 + 2.5, both bored from this same face. 0.250 in would need
        // short inserts AND a button-head screw.
        //
        // Heat-set inserts, blind from the MATING face. Nothing is counterbored
        // and nothing passes through to the device side: the bolts come up from
        // underneath, through the arm pad's overhanging ears, and the inserts
        // are all this plate contributes. That is what makes the joint
        // serviceable with the repeater still bolted on - see the head.
        // They sit at y = +-32, well outboard of the +-12 contact strip and of
        // the device's own two bolts on the centreline.
        // Must match the head exactly, shift included - the pattern is not
        // centred on the pad, it sits up-plane of it. See the head's pad.
        const ix = [-ifBoltShift - bY / 2.0, -ifBoltShift + bY / 2.0];
        const iy = [-bX / 2.0, bX / 2.0];
        for (var i = 0; i < 2; i += 1)
            for (var j = 0; j < 2; j += 1)
                cuts = append(cuts, cylZ(context, pid, "ins" ~ toString(i) ~ toString(j),
                            ix[i], iy[j], -1.0, insDep, insD / 2.0));

        // Device bolts, through everything, COUNTERBORED ON THE BACK for socket
        // head cap screws. The heads land on this plate's mating face, and both
        // holes fall inside the arm pad's footprint, so a head left standing
        // proud holds the plate off the pad and the whole joint rocks on two
        // bolt heads. They go in before the plate meets the arm.
        cuts = append(cuts, cylZ(context, pid, "dh0", padUp - hsp / 2.0, 0,
                    -1.0, t + proud + 1.0, hd / 2.0));
        cuts = append(cuts, cylZ(context, pid, "dh1", padUp + hsp / 2.0, 0,
                    -1.0, t + proud + 1.0, hd / 2.0));
        cuts = append(cuts, cylZ(context, pid, "dc0", padUp - hsp / 2.0, 0,
                    -1.0, hCbDep, hCbD / 2.0));
        cuts = append(cuts, cylZ(context, pid, "dc1", padUp + hsp / 2.0, 0,
                    -1.0, hCbDep, hCbD / 2.0));

        cutWith(context, pid, "cuts", plate, cuts);

        if (cornerR > 0.01)
        {
            // ONLY the four outer corners. qParallelEdges over the whole body
            // also returns the rib slot's ends, the cable slot's corners and the
            // lip - fourteen edges in one all-or-nothing operation. The rib slot
            // is 8.3 mm wide and its edges are 3.3 mm long, so it cannot take a
            // fillet at any useful radius, and that one edge failed all of them.
            var picks = [];
            for (var e in evaluateQuery(context, plate->qOwnedByBody(EntityType.EDGE)))
            {
                const ok = try silent(edgeIsOuterCorner(context, e, -half, half, half, 0.6));
                if (ok == true)
                    picks = append(picks, e);
            }
            var done = false;
            if (size(picks) > 0)
                done = try silent(filletEdgeSet(context, pid + "fil",
                            qUnion(picks), cornerR)) == true;
            if (!done)
            {
                // Per-edge, so one stubborn corner cannot take the other three
                // with it.
                for (var i = 0; i < size(picks); i += 1)
                {
                    const one = try silent(filletEdgeSet(context, pid + ("fil" ~ toString(i)),
                                picks[i], cornerR));
                    done = done || (one == true);
                }
            }
            if (!done && size(picks) > 0)
                reportFeatureWarning(context, id,
                    "No corner would take a " ~ toString(cornerR) ~ " mm fillet.");
        }

        setProperty(context, { "entities" : plate, "propertyType" : PropertyType.NAME, "value" : "Top plate" });
    }, {
            "plateT" : 10.0 * millimeter,
            "plateSize" : 80.0 * millimeter,
            "cornerR" : 5.0 * millimeter,
            "padUp" : 5.0 * millimeter,
            "holeSpacing" : 40.0 * millimeter,
            "holeD" : 6.5 * millimeter,
            "stripW" : 24.0 * millimeter,
            "stripProud" : 0.0 * millimeter,
            "ifBoltX" : 64.0 * millimeter,
            "ifBoltY" : 26.0 * millimeter,
            "ifBoltShift" : 10.0 * millimeter,
            "holeCbD" : 11.0 * millimeter,
            "holeCbDep" : 6.3 * millimeter,
            "insertD" : 5.8 * millimeter,
            "insertDepth" : 8.0 * millimeter
        });
