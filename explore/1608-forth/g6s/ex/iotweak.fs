\ tweak io primitives using the BSRR register

forgetram

include ../../flib/stm32f1/io.fs

omode-pp pb9 io-mode!

: toggle-perf0
  micros
  100000 0 do
  loop
  micros swap - . ;

: toggle-perf1
  micros
  100000 0 do
    i pb9 io!
  loop
  micros swap - . ;

include ../../flib/stm32f1/io-orig.fs

: toggle-perf2
  micros
  100000 0 do
    i pb9 io!
  loop
  micros swap - . ;

: io! ( f pin -- )  \ set pin value
  swap 0= $10 and +
  dup io-mask swap io-base GPIO.BSRR + ! ;

: toggle-perf3
  micros
  100000 0 do
    i pb9 io!
  loop
  micros swap - . ;

(  empty loop: ) toggle-perf0
(   optimised: ) toggle-perf1
(   dumb code: ) toggle-perf2
( faster code: ) toggle-perf3
