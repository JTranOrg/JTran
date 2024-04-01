## Test APIs
  These REST apis show different uses of JTran

### Madrid
 
##### GET /madrid/municipalities
Returns a list of municipalities within the community of Madrid (a community is akin to a county). The JTran transform takes the original [Spanish source](https://datos.comunidad.madrid/catalogo/dataset/032474a0-bf11-4465-bb92-392052962866/resource/301aed82-339b-4005-ab20-06db41ee7017/download/municipio_comunidad_madrid.json) and translates it to English (the property names not the data) 

##### GET /madrid/municipality/:code
Returns the municipality with the given code