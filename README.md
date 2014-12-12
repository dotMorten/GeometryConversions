GeometryConversions
===================

Provides conversion between ArcGIS Runtime Geometry and other Geometry data objects.


 Currently supported dataformats:
 
 - To and from Well-known Binary
 - To and from System.Spatial Geometry Type
 - To and from System.Spatial Geography Type
 - To and from Sql Server Spatial Geometry Type (Windows Desktop only)
 - To and from Sql Server Spatial Geography Type (Windows Desktop only)


[System.Spatial](http://www.nuget.org/packages/System.Spatial/) supports conversion for GML, GeoJSON and Well-known Text*, so these formats can be accomplished using System.Spatial as an intermediary format. Future plans might include direct conversion support to reduce overhead.




<br/><br/><br/><br/>

*Please use Well-Known Binary instead of Well-Known Text. Every time someone uses WKT instead of WKB, a baby-seal gets clubbed to death.
