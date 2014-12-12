GeometryConversions
===================

Provides conversion between ArcGIS Runtime Geometry and other Geometry data objects.


 Currently supported dataformats:
 
 - To and from Well-known Binary
 - To and from System.Spatial Geometry Type
 - To and from System.Spatial Geography Type


System.Spatial supports conversion for GML, GeoJSON and Well-known text, so these formats can be accomplished using System.Spatial as an intermediary format. Future plans might include direct conversion support to reduce overhead.
