[![Build status](https://ci.appveyor.com/api/projects/status/3voth0yob3rnpx70/branch/master?svg=true)](https://ci.appveyor.com/project/richorama/odataservice/branch/master)

Two10degrees OData Service
===========================

A console application which starts an HTTP server, allowing the user to configure database queries exposed by the service as OData endpoints.

Installation
------------

1. Copy all the files, onto your local system.
2. Create a database, and run the CreateTables.sql script to create the necessary tables.
3. Update the .config file, and set the connectionString property to the appropriate database.
4. Start the Two10.ODataService.exe application.

 > The service will start on port 8080 by default, but an alternative port can be selected by passing the number in as the first argument on the command line.

Running
-------

Start the Two10.ODataService.exe application, and open your web browser at http://localhost:8080/. (unless you have selected an alternative port number). 

The design page should appear. If you have any existing endpoints configured, they will appear here. You can create a new endpoint by selecting the 'New' link. 

Creating an endpoint
--------------------

Three pieces of information are required to create your endpoint:

1. A unique name, to identify your endpoint.
2. A connection string to the database you wish to query.
3. A query which returns the data you wish to expose as OData.

Pressing the 'Save' button will create the OData feed.

 > The OData feed requires an ID or primary key for your query. It's impossible from the query results to determine which field is the primary key. The service uses convention to discover this. It inspects the field names, and selects the first field which ends with 'id'. If no fields match this criteria, the first field is used.

Supported OData Queries
-----------------------

Clicking on the 'Try the OData service' link to view services you have configured in the OData XML format:

	http://localhost:8080/Data

Response:

	<?xml version="1.0" encoding="iso-8859-1" standalone="yes"?>
	<service xml:base="http://localhost:8080/Data" xmlns:atom="http://www.w3.org/2005/Atom" xmlns:app="http://www.w3.org/2007/app" xmlns="http:// www.w3.org/2007/app">
	  <workspace>
	  <atom:title>Default</atom:title>
	  <collection href="/Data/DataService1">
	    <atom:title>DataService1</atom:title>
	  </collection>
	  <collection href="/Data/DataService2">
	    <atom:title>DataService2</atom:title>
	  </collection>
	  </workspace>
	</service>

To view all records in a service, add the dataservice name to the URL: 

	http://localhost:8080/Data/DataService1

Response:

	<?xml version="1.0" encoding="iso-8859-1" standalone="yes"?>
	<feed xml:base="http://localhost:8080/Data/" xmlns:d="http://schemas.microsoft.com/ado/2007/08/dataservices" xmlns:m="http://schemas.microsoft.com/ado/2007/08/dataservices/metadata" xmlns="http://www.w3.org/2005/Atom">
		<title type="text">DataService1</title>
		<id>http://localhost:8080/Data/DataService1</id>
		<updated>2011-08-03T13:52:45Z</updated>
		<link rel="self" title="DataService1" href="DataService1" />
		    <entry>
		      <id>http://localhost:8080/Data/DataService1(1)</id>
		      <title type="text"></title>
		      <updated>2011-08-03T13:52:45Z</updated>
		      <author>
		        <name />
		      </author>
		      <link rel="edit" title="Test" href="DataService1(1)" />
		      <category term="Namespace.Test" scheme="http://schemas.microsoft.com/ado/2007/08/dataservices/scheme" />
		      <content type="application/xml">
		        <m:properties>
		          <d:Field1>Value1</d:Field1>
		          <d:Field2>Value2</d:Field2>
		          <d:Field3>Value3</d:Field3>
		          <d:Field4>Value4</d:Field4>
		        </m:properties>
		      </content>
		    </entry>
		    ...

A single entry can be retrieved by specifying the Id in the brackets: 

	http://localhost:8080/Data/DataService1(1)

Response:

	<entry>
	  <id>http://localhost:8080/Data/DataService1(1)</id>
	  <title type="text"></title>
	  <updated>2011-08-03T13:52:45Z</updated>
	  <author>
	    <name />
	  </author>
	  <link rel="edit" title="Test" href="DataService1(1)" />
	  <category term="Namespace.Test" scheme="http://schemas.microsoft.com/ado/2007/08/dataservices/scheme" />
	  <content type="application/xml">
	    <m:properties>
	      <d:Field1>Value1</d:Field1>
	      <d:Field2>Value2</d:Field2>
	      <d:Field3>Value3</d:Field3>
	      <d:Field4>Value4</d:Field4>
	    </m:properties>
	  </content>
	</entry>

To see a single property, specify this on the end of the path:

	http://localhost:8080/Data/DataService1(1)/Field1

Response:

	<Field1 xmlns="http://schemas.microsoft.com/ado/2007/08/dataservices">Value1</Field1>

To view the raw value, specify $value on the end of the path:

	http://localhost:8080/Data/DataService1(1)/Field1/$value

Response:

	Value1

To retrieve the number of records, add $count to the path:

	http://localhost:8080/Data/DataService1/$count

Response:

	45

Queries can be filtered using simple expressions appended a $filter to the query part of the URL:

	http://localhost:8080/Data/DataService1?$filter=Field1 eq Value1

The following expressions are supported:

	eq  =   (equals)
	lt  <   (less than)	
	gt  >   (greater than)
	le  <=  (less than or equal)
	ge  >=  (greater than or equal)
	ne  !=  (not equal)

Records can be sorted by appending $orderby to the query part of the URL:

	http://localhost:8080/Data/DataService1?$orderby=Field1,Field2 desc

A record limit can be introduced using $top, which will return the first n records:

	http://localhost:8080/Data/DataService1?$top=100

The properties returned can be selected using $select:

	http://localhost:8080/Data/DataService1?$select=Field1,Field2

The '$' keywords can be used in any combination, for example:

	http://localhost:8080/Data/DataService1?$top=100&$select=Field1,Field2&orderby=Field1,Field2&$filter=Field3 eq 'Value3'

An extra (non-OData) keyword has been added, $sql, which will display the query which will be executed. This is useful for debugging the service:

	http://localhost:8080/Data/DataService1?$sql

Response:

	SELECT  * FROM (select * from Example) AS SubQuery  


Known Limitations
-----------------

1. JSON is not currently supported, all responses are in XML.
2. Advanced filtering is not available.
3. Joins are not available.
4. Pagination is not supported.

License
-------
 MIT
