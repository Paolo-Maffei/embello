
\ TvE init
: ti
  912500000 rf.freq ! 6 rf.group ! rf-init $1f rf-power
  rf. cr
  ;

\ Send Hello World messages non-stop
: t
  ti
  \ 03 05 rf! $4E 6 rf!  rf.
  begin
    s" Hello World" 0 rf-send
    \ 100 ms
  again ;

: .n ( n -- ) \ print signed integer without following space
  dup abs 0 <# #S rot sign #> type ;

: rf>uart ( len -- len )  \ print reception parameters
  rf.freq @ .n ." Hz "
  rf.rssi @ 2/ negate .n ." dBm "
  rf.afc @ 16 lshift 16 arshift 61 * .n ." Hz "
  dup .n ." b "
  ;

\ Send Hello World and expect an ACK
: ta
  ti
  begin
    s" Hello World" $80 rf-send
    rf-recv drop
    1
    1000 0 do
      rf-recv ?dup if
        ." RF69 " rf>uart rf-correct
        0 do
          rf.buf i + c@ h.2
        loop cr
        drop 0
      then
      100 us
    loop
    if ." lost" cr then
    RF:PA rf@ $1f xor RF:PA rf!
    \ RF:SYN3 rf@ not RF:SYN3 rf!
    300 ms
  again ;

: low-power-sleep ( n -- ) \ sleep n * 100ms at low power
  rf-sleep
  -adc \ only-msi
  0 do stop100ms loop
  hsi-on adc-init ;

\ Send Hello World and expect an ACK
: tb
  ti
  begin
    s" Hello World" $80 rf-send
    20 rf-ack? ?dup if
      rx-connected? if
        rf-sleep
        PA0 ios!
        ." RF69 " rf>uart
        0 do
          rf.buf i + c@ h.2
        loop
        PA0 ioc!
      then
      rf-correct
      LED ioc!
    else ." lost" then cr
    \ RF:PA rf@ $1f xor RF:PA rf!
    \ RF:SYN3 rf@ not RF:SYN3 rf!
    1 low-power-sleep
    LED ios!
    600 low-power-sleep
  again ;

: main
  2.1MHz  1000 systick-hz  lptim-init adc-init
  OMODE-PP PA0 io-mode!
  tb ;
