Temperature sensor reporting over the radio
===========================================

## JZ1 w/rfm69

Initializing a JeeNode Zero rev1 (e.g. jnp-rfm167-v1) from scratch:
```
load mecrisp from embello/explore/1608-forth/cores/l052-mecrisp.bin
!u https://github.com/jeelabs/embello/blob/master/explore/1608-forth/cores/l053-mecrisp-2.3.5.bin?raw=true
!s ../jz3/always.fs
!s ../jz1/board.fs
!s core69.fs
!s rftemp.fs
main
```

## JZ3 w/rfm69

Initializing a JeeNode Zero rev3 (e.g. jnz-69/7x-v3) from scratch:
```
load mecrisp from embello/explore/1608-forth/cores/l052-mecrisp.bin
!s ../jz3/always.fs
!s ../jz3/board.fs
!s core69.fs
!s rftemp.fs
main
```

## Packet format

hwid, temp[cC], pressure[Pa], rh[%], light[lux], cpu\_temp[C], Vstart[mV], Vend[mV]

Notes: temp is hundredths degrees Centigrade, Vend is of _previous_ packet send.

## Nodes

 ID | hwid     | pgm     | date       | tx   | location       | notes
 --------------------------------------------------------------------
  1 | 32390242 | rf-temp | 2017-03-18 | 2min | GH, top-box    | i2c-bb, bme-not-off
  2 | 323B0254 | rf-temp | 2017-03-18 | 2min | GH, grafts     | i2c-bb, bme-not-off
