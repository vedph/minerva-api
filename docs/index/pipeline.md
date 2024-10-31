# Indexing Pipeline

This section provides a detailed explanation for the [configuration document](atti-chiari.json) of the indexing pipeline.

- [Indexing Pipeline](#indexing-pipeline)
  - [TEI Description](#tei-description)
  - [Profile](#profile)
    - [1. Source](#1-source)
    - [2. Text Filters](#2-text-filters)
    - [3. Attribute Parsers](#3-attribute-parsers)
    - [4. Document Sort Key](#4-document-sort-key)
    - [5. Document Date Value](#5-document-date-value)
    - [6. Tokenizer](#6-tokenizer)
    - [7. Structure Parser](#7-structure-parser)
      - [About Phrases](#about-phrases)
    - [8. Text Retriever](#8-text-retriever)
    - [9. Text Mapper](#9-text-mapper)
    - [10. Text Picker](#10-text-picker)
    - [11. Text Renderer](#11-text-renderer)

## TEI Description

The TEI documents have a minimalist header, as no sensitive information should be found in them (a separate file with metadata is stored independently). So, all what the header includes is the original file name.

The TEI document's body is the outcome of a conversion process starting from a Word document with no headings or other structural divisions, given the nature of these texts. As such, the `body` element just includes paragraphs. Some essential features of the original document styles are preserved in TEI, like bold, italic, underline, and text alignment. Also, all the portions processed by the anonymizer tool have been marked, e.g. as abbreviations, personal names, place names, juridic person names, dates, alphanumeric codes, numbers, foreign language parts, etc., using the proper TEI elements.

For instance, here is a sample paragraph:

```xml
<p rend="j"><hi rend="b"><orgName type="f">COMUNARDA</orgName> <choice><abbr>S.p.A.</abbr><expan xml:lang="ita">Società per Azioni</expan></choice></hi>, in persona del legale rappresentante <choice><abbr>p.t.</abbr><expan xml:lang="lat">pro tempore</expan></choice>, elettivamente domiciliata in <placeName>Leivi</placeName>, alla Via <address><addrLine>Yenna Vicenzo, 89</addrLine></address>, presso e nello studio dell’<choice><abbr>avv.</abbr><expan xml:lang="ita">avvocato</expan></choice> <persName type="mn">Almirante</persName> <persName type="s">Masuero</persName>, che la rappresenta e difende</p>
```

Metadata about each document (not including any personal information) are stored in a separate file, which originally is an Excel (XLSX) file having a single sheet with 2 columns, as this format is preferred by operators more familiar with office-like applications. Each row in this file represents a name=value pair. For instance, here are some rows, here represented with CSV:

```txt
atto,Atto di citazione appello
data,20091100
giudicante,Corte di Appello
grado,2
gruppo-atto,GE_t
gruppo-nr,01
id,full|ge|civ-ge-app-cit342-200911_01.xml
materia,civile
nascita-avv,1955
sede-giudicante,Genova
sede-raccolta,ge
sesso-avv,M
```

These metadata are preprocessed by the same tool used to pseudonymize the documents (`an_tool`). This converts XLSX into CSV, while checking that all the metadata listed are recognized, their values are valid, and that all the required metadata are present. It can also rename metadata, merging differently named metadata into one, and map their values into other values where required. All the parameters for these checks are defined in an external JSON file, so that the tool can be reused.

So, in the end what Pythia has to ingest is a set of TEI files, each having a corresponding metadata CSV file. We want to extract these metadata, and attach them to the corresponding document for indexing purposes. This will allow filtering documents by their metadata, and including these filters also in text searches.

## Profile

The Pythia indexing profile is just a JSON file. You can write it with your favorite text/code editor (mine is [VSCode](https://code.visualstudio.com)).

### 1. Source

In our case, the source for documents is just the file system, assuming that we have placed our documents in some folder. So we use a `source-collector.file`, which lists all the files found in a specific folder, here without recursion:

```json
"SourceCollector": {
  "Id": "source-collector.file",
  "Options": {
    "IsRecursive": false
  }
},
```

### 2. Text Filters

These filters preprocess the source texts before indexing. Filters are applied in the order they are specified:

- `text-filter.xml-local-tag-list` (for UDPipe): used to extract a list of XML tags, one for each of the tags found in the source text, and listed in the filter's options. For each tag found it stores its name and position. Such data will be used later by other components in the pipeline. In this case, these are the UDPipe components, which must ignore any text inside TEI tags like `abbr` or `num`. This avoids garbage text derived from annotations creeping into the main text flow, thus providing a correct text to the UDPipe service.

```json
{
  "Id": "text-filter.xml-local-tag-list",
  "Options": {
    "Names": [
      "abbr",
      "num"
    ]
  }
},
```

- `text-filter.xml-tag-filler` is used to blank-fill the whole TEI `expan` element, as we do not want its text to be handled as document's text. In fact, the content of `expan` is just the expansion of an abbreviation in the text, so we exclude it from indexing. In its `Tags` property, we list all the tag names of the elements to be blank-filled; in this case, we just have `expan`. As `expan` belongs to the TEI namespace, we also have to add it in `Namespaces`, so that `tei:expan` gets resolved and the XML element gets its correct namespace.

```json
{
  "Id": "text-filter.xml-tag-filler",
  "Options": {
    "Tags": [
      "tei:expan"
    ],
    "Namespaces": [
      "tei=http://www.tei-c.org/ns/1.0"
    ]
  }
},
```

- `text-filter.tei` is used to discard the whole header from the text index. This avoids indexing the header's text as document's text.

```json
{
  "Id": "text-filter.tei"
},
```

- `text-filter.replacer` is used to apply a minor adjustment to source texts by means of string or pattern replacements. In this case, the only adjustment is replacing `E’` (preceded by word boundary) to `È`. This is required because in some cases the documents authors have misused this quote as an accent marker, and failing to mark it properly would have negative effects on POS tagging (e.g. `è` being a verb, and `e` a conjunction).

```json
{
  "Id": "text-filter.replacer",
  "Options": {
  "Replacements": [
      {
        "Source": "\\bE’",
        "IsPattern": true,
        "Target": "È "
      }
    ]
  }
},
```

- `text-filter.quotation-mark` is used to ensure that apostrophes are handled correctly, by replacing "smart quotes" with an apostrophe character proper.

```json
{
  "Id": "text-filter.quotation-mark"
},
```

- `text-filter-udp` is used to submit the whole text (as filtered by the preceding filters) to the [UDPipe service](https://lindat.mff.cuni.cz/services/udpipe/) for POS tagging. The options specify the model used for the Italian language, the blacklisted tags to exclude from tagging (`abbr` and `num` as collected by the first filter above), and the text chunks sizes and tail detection expression. The filter does not effectively changes the received text, but just collects the POS tagger results to be consumed later at the token level. We require to do this at the text level, as POS tagging works on whole sentences; but we will consume the results later, so that we can add metadata to each single token as we detect and filter them.

```json
{
  "Id": "text-filter.udp",
  "Options": {
    "Model": "italian-isdt-ud-2.12-230717",
    "MaxChunkLength": 5000,
    "ChunkTailPattern": "(?<![0-9])[.?!](?=\\s|$)",
    "BlackTags": [
      "abbr",
      "num"
    ]
  }
}
```

### 3. Attribute Parsers

Attribute parsers extract metadata from text for the objects in the index.

- `attribute-parser.xml` is used to extract metadata from the TEI header. The only datum here is the document's `title`, because for security reasons all the metadata are stored separately.

```json
{
  "Id": "attribute-parser.xml",
  "Options": {
    "Mappings": [
      "title=/tei:TEI/tei:teiHeader/tei:fileDesc/tei:titleStmt/tei:title"
    ],
    "DefaultNsPrefix": "tei",
    "Namespaces": [
      "tei=http://www.tei-c.org/ns/1.0"
    ]
  }
},
```

- `attribute-parser.fs-csv` is used to parse the CSV metadata file corresponding to the document file. This has the same name of the document file, with the additional suffix `.meta`. So, to get the metadata file corresponding to the source file we instruct the parser to build the file path starting from the source file path and replacing its `.xml` extension with `.xml.meta`. Also, we tell the parser to look for metadata _names_ in column 0, and for their _values_ in column 1, trimming them if required.

```json
{
  "Id": "attribute-parser.fs-csv",
  "Options": {
    "NameColumnIndex": 0,
    "ValueColumnIndex": 1,
    "ValueTrimming": true,
    "SourceFind": "\\.xml$",
    "SourceReplace": ".xml.meta"
  }
}
```

We are thus collecting metadata for documents from two different sources: the document itself (from its TEI header), and an independent CSV file.

>Pythia also provides an XSLX (Excel) attribute parser, but here the conversion from the original Excel files has already been done by the pseudonymizer tool, which is also in charge of ensuring that metadata are uniform and valid.

### 4. Document Sort Key

The standard sort key builder (`doc-sortkey-builder.standard`) is used to build the sort key for each indexed document. It sorts documents by author (which for these documents is always empty), title, and date.

```json
"DocSortKeyBuilder": {
  "Id": "doc-sortkey-builder.standard"
},
```

### 5. Document Date Value

The document date value calculator specified here is used to calculate a numeric value representing the document's date.

The document's date is in metadata, from the `data` attribute, in the form `YYYYMMDD`. We want only the year, so we use a year calculator to extract just the year value from this date by parsing it via a regular expression.

```json
"DocDateValueCalculator": {
  "Id": "doc-datevalue-calculator.year",
  "Options": {
    "Attribute": "data",
    "Pattern": "^(?<y>\\d{4})"
  }
},
```

### 6. Tokenizer

We use here a standard tokenizer (`tokenizer.standard`) which splits text at whitespace or apostrophes (while keeping the apostrophes with the token). Its **filters** are:

- `token-filter.punctuation`: this filter collects metadata about punctuation at the left/right boundaries of each token. Here we provide a whitelist of punctuation characters to include (rather than letting the filter collect any character categorized as punctuation in Unicode): `ListType=1` means that the list is a whitelist rather than a blacklist (-1).

```json
{
  "Id": "token-filter.punctuation",
  "Options": {
  "Punctuations": ".,;:!?",
  "ListType": 1
  }
},
```

- `token-filter.udp`: this filter consumes the [UDPipe]((https://lindat.mff.cuni.cz/services/udpipe/)) analysis results collected by `text-filter-udp` and maps them to the tokens defined by the Pythia's pipeline's tokenizer; for each matched token it adds it a selection of its POS tags as attributes. The `language` property set to an empty string means that such attributes should be added only to tokens having a null (non-specified) language. In this corpus in fact we use data derived from TEI `foreign` to mark tokens as non-Italian, assuming that any unmarked token is Italian. So, the effect of this setting is allowing UDP-derived attributes only for Italian tokens, leaving out all the tokens marked for another language.

```json
{
  "Id": "token-filter.udp",
  "Options": {
    "Props": 43,
    "Language": ""
    }
},
```

The `Props`=43 (=0x2B=0010 1011: `lemma`, `upos`, feats, `deprel`) property defines the UDP properties to be mapped into token attributes. Each is a bit value:

- 0x01=`lemma`;
- 0x02=`upos`;
- 0x04=`xpos`;
- 0x08=feats, meaning that for each feature a corresponding attribute will be created;
- 0x10=`head`;
- 0x20=`deprel`;
- 0x40=`misc`.

- `token-filter.ita`: this filter removes all the characters which are not letters or apostrophe, strips from them all diacritics, and lowercases all the letters.

```json
{
  "Id": "token-filter.ita"
},
```

- `token-filter.len-supplier`: this filter does not touch the token, but adds metadata to it, related to the number of letters of each token.

```json
{
  "Id": "token-filter.len-supplier",
  "Options": {
    "LetterOnly": true
  }
}
```

### 7. Structure Parser

Structure parsers define text spans larger than tokens.

- `structure-parser.xml`: a general-purpose filter for XML documents, used to extract a number of different structures corresponding to main text divisions. Unfortunately, our documents have no intrinsic structure. We can only rely on paragraphs, so this is the only structure detected. Yet, there are a number of "ghost" structures, i.e. text spans which are not to be indexed as such, but can be used to extract more metadata for the tokens they include. This happens e.g. for `persName` and all the others listed below, so that we can add metadata to the corresponding word(s) telling us that they are anthroponyms or whatever else. This will allow us searching for tokens which are e.g. person names, or geographic names, etc. Finally, the structure parser also uses a standard structure filter (`struct-filter.standard`) to properly extract their names from the original text by stripping out rumor features (diacritics, case, etc.).

Structures:

- `p`: paragraph. The value is the paragraph length.
- `fp-lat` (foreign phrase, Latin): Latin phrase. The value is the trimmed phrase text.

Ghost structures produce these token attributes:

- `abbr`=`1`
- `address`=`1`
- `date`=(trimmed date text)
- `email`=`1`
- `foreign`=(language)
- `b`=`1` for bold
- `i`=`1` for italic
- `u`=`1` for underline
- `n`=`1` for numbers
- `org-m`=(trimmed name text) for male organization names
- `org-f`=(trimmed name text) for female organization names
- `pn-m`=(trimmed name text) for male person names
- `pn-f`=(trimmed name text) for female person names
- `pn-s`=(trimmed name text) for surnames

>As you can see from the XPath expressions used to select structures, some of them also rely on the value of attributes to detect a specific token type. For instance, `persName` has `@type`=`mn` for male name, `fn` for female name, `s` for surname. So, by using different mappings we can preserve such finer distinctions for the index.

The `PrivilegedMappings` property lists the token attributes which target a privileged attribute. This is the case of `foreign`, which gets mapped to the `language` privileged attribute.

The `Filters` property lists the filters applied to the text extracted by the parser as the value. Here we just use a standard structure filter. This removes any character different from letter or digit or apostrophe or whitespace, any diacritics from each letter, and lowercases each letter. Whitespaces are all normalized to a single space, and the result is trimmed.

- `structure-parser.xml-sentence`: a sentence parser. This relies on punctuation (and some XML tags) to determine the extent of sentences in the document. The parser has as its root XML element the TEI `body` element. To detect sentence boundaries it simply relies on a combination of punctuation characters plus XML elements. The punctuation characters used as end sentence markers are defined by `SentenceEndMarkers` (stop mark, exclamation mark, question mark); the XML closing tags which count as sentence end markers are listed by `StopTags` (here just `head`, representing a title). Additionally, any sentence end marker punctuation found in XML elements `abbr` and `num` is discarded. This is because abbreviations or numbers might include dots, which would be misleading for sentence end detection.

```json
{
  "Id": "structure-parser.xml-sentence",
  "Options": {
    "RootXPath": "/tei:TEI/tei:text/tei:body",
    "StopTags": ["head"],
    "NoSentenceMarkerTags": ["abbr", "num"],
    "SentenceEndMarkers": ".!?",
    "Namespaces": ["tei=http://www.tei-c.org/ns/1.0"]
  }
}
```

#### About Phrases

The `foreign` configuration requires some more explanation. Here we are effectively using a ghost structure to assign the language to each token in it, but also a couple of true structures to collect Latin phrases as a whole. The first property is:

```json
{
  "Name": "foreign",
  "XPath": "//tei:foreign",
  "ValueTemplate": "{l}",
  "ValueTemplateArgs": [
    {
      "Name": "l",
      "Value": "./@xml:lang"
    }
  ],
  "TokenTargetName": "foreign"
},
```

This means: "whenever you find (`XPath`) a TEI `foreign` element, create a structure named `foreign` (`Name`), whose value is specified by `ValueTemplate`. This is a string where optional variables (the arguments of the template) are represented with placeholders delimited by braces and including the variable name. Variables are defined in `ValueTemplateArgs`. Here we have attribute `xml:lang` of element `foreign`, i.e. the foreign word language, like `lat`=Latin, or `eng`=English.

So, whenever we find `<foreign xml:lang="lat">pro tempore</foreign>`, a structure named `foreign` is created with value `lat`. This is a ghost structure, so it exists only for the purpose of assigning the `foreign` attribute to its tokens. The structure itself will not be saved. It is the `TokenTargetName` attribute which makes this a ghost structure, by specifying the name of the attribute for the target tokens, which are all the tokens inside the ghost structure.

Anyway, here we also want to get true structures representing the whole foreign language phrase. So, we add another property:

```json
{
  "Name": "fp-lat",
  "XPath": "//tei:foreign[@xml:lang='lat']",
  "ValueTemplate": "{txt}",
  "ValueTemplateArgs": [
    {
      "Name": "txt",
      "Value": "./text()"
    }
  ],
  "ValueTrimming": true
},
```

This defines a true structure which will be saved among spans. Its name is `fp-lat` (foreign phrase, Latin), and it matches any TEI `foreign` element with Latin language. The structure value here is equal to the textual content of the element, with trimming. In most cases structures do not have a value, or at least their value is not their text, because this is pointless; for instance, almost every sentence structure would be different from the others, very long, and almost unpredictable, so that it would make no sense storing its value, which further would not be found unless typed exactly as it is. Yet, in the case of phrases we do want the value right because this is formulaic, and thus we expect it to be almost always the same for each phrase, like "pro tempore". This way, we allow queries like:

```txt
[$fp-lat] AND [_value="pro tempore"]
```

where the first pair says we are looking for a structure named `fp-lat`, and the second pair that the value of this structure (whence `_` before `value`, meaning that we are targeting the structure attributes) must be "pro tempore". Of course, we can still use `[$fp-lat]` alone to find any Latin phrase, whatever its value.

Of course, this structure, like any other, is just a span. So you can build queries which treat it like you would do with simple tokens. For instance, this is a query based on tokens only:

```txt
([value="una"] OR [value="nella"]) BEFORE(m=0) [value="fattispecie"]
```

This finds words "una" or "nella", before word "fattispecie". With a phrase, we would do the same, like e.g.:

```txt
([$fp-lat] AND [_value="pro tempore"]) BEFORE(m=0) [value="elettivamente"]
```

This finds all the Latin phrases whose value is "pro tempore" before word "elettivamente".

### 8. Text Retriever

A file-system based text retriever (`text-retriever.file`) is all what we need to get the text from their source. In this case, the source is a directory, and each text is a file.

```json
"TextRetriever": {
  "Id": "text-retriever.file"
}
```

Note that this retriever is used to _build_ the index. Once the index has been built, you might continue using it to retrieve the texts from their original source, but it would make the index less portable and dependent from a file system. So, we rather include the texts in the index itself, and later replace this retriever with another targeting a RDBMS. In our case, this will be `text-retriever.sql.pg` (for PostgreSQL). This is the preferred choice as it makes the database self-contained and the index independent from the host file system. In this case, you will just replace the ID of the text retriever after indexing all the files.

### 9. Text Mapper

A map generator for XML documents (`text-mapper.xml`), based on the specified paths on the document tree. Here we just pick the `body` element as the map's root node, and its children `p` elements as children nodes, as in these documents they represent the only available divisions.

```json
"TextMapper": {
  "Id": "text-mapper.xml",
  "Options": {
    "Definitions": [
      {
        "Name": "body",
        "XPath": "/tei:TEI/tei:text/tei:body",
        "ValueTemplate": "act"
      },
      {
        "Name": "p",
        "ParentName": "body",
        "XPath": "./tei:p",
        "DefaultValue": "paragraph",
        "ValueTemplate": "{t}",
        "ValueTemplateArgs": [
          { "Name": "t", "Value": "." }
        ],
        "ValueMaxLength": 60,
        "ValueTrimming": true,
        "DiscardEmptyValue": true
      }
    ],
    "Namespaces": ["tei=http://www.tei-c.org/ns/1.0"],
    "DefaultNsPrefix": "tei"
  }
},
```

As paragraphs usually have a long text, we cut it at maximum 60 characters, while also normalizing all their whitespaces. This way they will serve as nodes in the document map, allowing users to browse across the document's content.

### 10. Text Picker

The text picker is the component in charge of extracting a somehow semantically meaningful portion from a text. Here we use a picker for XML documents (`text-picker.xml`).

As the default namespace in these TEI documents is the TEI namespace, we must notify the picker about it using `DefaultNsPrefix`, which defines the default namespace prefix. In turn, this is mapped to the TEI namespace in `Namespaces`, among other namespaces found in the documents (like the XML namespace itself for attributes like `lang`). This way, even when the picker extracts a sub-portion of the original document like a single paragraph, it will be assigned the default TEI namespace defined in the root element.

```json
"TextPicker": {
  "Id": "text-picker.xml",
  "Options": {
    "HitOpen": "<hi rend=\"hit\">",
    "HitClose": "</hi>",
    "Namespaces": [
      "tei=http://www.tei-c.org/ns/1.0",
      "xml=http://www.w3.org/XML/1998/namespace"
    ],
    "DefaultNsPrefix": "tei"
  }
},
```

### 11. Text Renderer

The text renderer is the component which renders the document into some presentational format, here from XML to HTML, using a specified XSLT script.

```json
"TextRenderer": {
  "Id": "text-renderer.xslt",
  "Options": {
    "Script": "c:\\users\\dfusi\\desktop\\atti\\read.xsl",
    "ScriptRootElement": "{http://www.tei-c.org/ns/1.0}body"
  }
}
```

Again, note that this is the build configuration. After the index is built we rather use a profile which embeds the XSLT script so that it does not depend on the file system.

> 🛠️ Technical note: in this script the XML document gets transformed into HTML, referring to external [CSS styles](./example/read.css). It would not be possible to embed styles in the output, as in the frontend the output body gets extracted from the full HTML output, and dynamically inserted in the UI page. So, should you embed styles in the HTML header, they would just be dropped. The correct approach is rather defining global styles for your frontend app; these styles will then be automatically applied to the HTML code generated by the renderer. The XSLT script here wraps the text output into an `article` element with class `rendition`, so that all the styles selecting `article.rendition` are meant to be applied to the text renderer output.
