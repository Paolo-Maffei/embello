\ print fixed point integers

: .n ( n -- ) \ print signed integer without following space
  s>d <# #S #> type
  ;

: u.n ( u n -- ) \ print u using n characters (leading space)
  <#
  swap ( n u )
  begin \ output digits
    0 # drop \ output digit
    swap 1- swap \ decrement n
    dup 0= \ stop when number is zero
  until
  0 #> ( n c-addr len )
  rot spaces
  type
  ;

: fractional.n ( ncomma n -- ) \ print fractional using n characters
  <#
  begin
    1- dup 0 >= while
    swap f# swap
  repeat
  #> type
  ;

: f.n ( ncomma nwhole n m -- ) \ prints fixed-point using n int and m decimal digits
  rot dup 0< if ." -" rot 1- -rot then \ print minus sign, adjust n
  abs rot ( ncomma m nwhole n )
  u.n ( ncomma m )
  ." ."
  fractional.n ( )
  ;

: .centi ( n -- ) \ print signed int divided by 100 with 2 decimals without following space
  $80000000 swap 100,0 f/ 0 2 f.n
  ;

: .milli ( n -- ) \ print signed int divided by 100 with 2 decimals without following space
  $80000000 swap 1000,0 f/ 0 3 f.n
  ;

