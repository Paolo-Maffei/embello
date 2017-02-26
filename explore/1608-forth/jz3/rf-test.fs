
\ TvE init
: ti
  912500000 rf.freq ! 6 rf.group ! rf-init $08 rf-power
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

: rf-correct
  rf.afc @ 16 lshift 16 arshift 61 *         \ AFC correction applied in Hz
  \ 4392 -                                     \ adjust for AFC low beta offset
  2 arshift                                  \ apply 1/4 of measured offset as correction
  12000 over 0< if negate max else min then  \ don't apply more than 1/4 of Fdev
  rf.freq @ + dup rf.freq ! rf-freq          \ apply correction
  ;

: rf>uart ( len -- len )  \ print reception parameters
  rf.freq @ .n ." Hz "
  rf.rssi @ 2/ negate .n ." dBm "
  rf.fei @ 16 lshift 16 arshift 61 * .n ." Hz ("
  rf.afc @ 16 lshift 16 arshift 61 * .n ." Hz) "
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
    \ RF:SYN3 rf@ not RF:SYN3 rf!
    300 ms
  again ;


