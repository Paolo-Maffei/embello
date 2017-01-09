\ Helpers to work with character buffers

\ stack>buffer copies i bytes from the stack into a buffer
: stack>buffer ( b1 b2 ... bi i c-addr -- c-addr len )
  2dup swap 2>r \ save c-addr len
  over + ( b1 b2 ... bi i c-end-addr )
  swap 0 ?do 1- tuck c! loop
  drop 2r>
  ;

: buffer-cpy ( c-addr1 c-addr2 len -- c-addr1 len ) \ c-addr1 is dest c-addr2 is src
  tuck 3 pick swap 0 ?do ( c-addr1 len c-addr2 c-addr1 )
    over i + c@ over i + c!
  loop 2drop ;

: buffer. ( c-addr len -- ) \ print buffer like @<adr> <len> [ <c1> <c2> ... <cLen> ]
  ." @" over . dup . ." [ " 255 and 0 ?do dup i + c@ . loop drop ." ]" ;

: =buffers ( c-addr1 len1 c-addr2 len2 -- f )
  2 pick <> if ." len!=" 2drop drop 0 exit then
  swap 0 ?do
    dup i + c@  2 pick i + c@ <> if ." ch!= " unloop 2drop 0 exit then
  loop 2drop -1
  ;
