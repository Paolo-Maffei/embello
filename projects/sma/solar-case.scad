difference() {
    // outside
    cube([155,51,15]);
    // inside
    translate([3,3,1]) {
        cube([149,45,15]);
    }
    // rim
    translate([1,1,12]) {
        cube([153,49,5]);
    }
    // lcd cutout
    translate([12,13,-1]) {
        cube([72,25,5]);
    }
    // cable cutout
    translate([150,10,4]) {
        cube([20,5,15]);
    }
}