\ print fixed point integers

: >n ( n -- ) \ convert signed integer to string
  \ dup abs 0 <# #S rot sign #>
  dup dup 0< if abs then 0 <# #S rot sign #>
  ;

: .n ( n -- ) \ print signed integer without following space
  >n type
  ;

: s>n ( s n -- c-addr len ) \ convert signed number to n-character string
  <#
  over abs ( s n u )
  begin \ output digits
    0 # drop \ output digit
    swap 1- swap \ decrement n
    dup 0= \ stop when number is zero
  until
  rot 0< if sign swap 1- swap then \ if negative, output sign and decrement n
  0 #> ( n c-addr len )
  rot begin dup 0 > while 1- bl hold repeat
  ;

: f>n.m ( ncomma nwhole n m -- c-addr len ) \ convert fractional number to n-character string with m frac digits
  \ -rot over abs ( ncomma m nwhole n uwhole )
  -rot over dup 0< if abs then
  <#
  \ produce whole number digits
  begin \ output digits
    0 # drop \ output digit
    swap 1- swap \ decrement n
    dup 0= \ stop when number is zero
  until ( ncomma m nwhole n 0 )
  \ add sign and decrement n, if negative
  drop swap 0< if sign swap 1- swap then \ ( ncomma m n )
  -rot ( n ncomma m )
  \ add decimal point
  dup 0 > if [char] . hold< then \ add period if we're printing decimal digits
  \ add fractional digits
  begin
    1- dup 0 >= while
    swap f# swap
  repeat ( n ncomma 0 )
  rot begin dup 0 > while 1- bl hold repeat drop
  #> ( c-addr len )
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

: f.n.m ( ncomma nwhole n m -- ) \ prints fixed-point using n int and m decimal digits
  f>n.m type
  ;

: .centi ( n -- ) \ print signed int divided by 100 with 2 decimals without following space
  $80000000 swap 100,0 f/ 0 2 f.n.m
  ;

: >milli ( n -- ) \ convert signed int divided by 100 with 2 decimals without following space to string
  $80000000 swap 1000,0 f/ 0 3 f>n.m
  ;

: .milli ( n -- ) \ print signed int divided by 100 with 2 decimals without following space
  $80000000 swap 1000,0 f/ 0 3 f.n.m
  ;

