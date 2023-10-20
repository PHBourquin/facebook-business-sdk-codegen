/**
 * Copyright (c) Facebook, Inc. and its affiliates.
 * @format
 * 
 */

'use strict';

Object.defineProperty(exports, "__esModule", {
  value: true
});
const getBaseType = type => type.replace(/(^|^list\s*<\s*)([a-zA-Z0-9_\s]*)($|\s*>\s*$)/i, '$2');

const getTypeForCSharp = type => {
  if (!type) {
    return null;
  }

  // This is not perfect. But it's working for all types we have so far.
  const typeMapping = {
    string: /string|datetime/gi,
    bool: /bool(ean)?/gi,
    long: /(((unsigned\s*)?(\bint|long)))(?![a-zA-Z0-9_])/gi,
    double: /(float|double)/gi,
    'List<$1>': /list\s*<\s*([a-zA-Z0-9_.<>,\s]*?)\s*>/g,
    'Dictionary<$1, $2>': /map\s*<\s*([a-zA-Z0-9_]*?)\s*,\s*([a-zA-Z0-9_<>, ]*?)\s*>/g,
    '$1Dictionary<string, string>$2': /(^|<)map($|>)/g
  };

  let oldType;
  let newType = type;
  while (oldType !== newType) {
    oldType = newType;
    for (const replace in typeMapping) {
      newType = newType.replace(typeMapping[replace], replace);
    }
  }
  newType = newType.replace(/\<list\>/g, '<List<object>>');
  newType = newType.replace(/list$/g, 'List<object>');
  newType = newType.replace(/^file$/i, 'FileInfo');
  return newType;
};

const CodeGenLanguageCSharp = {
  formatFileName(clsName) {
    return clsName['name:pascal_case'] + '.cs';
  },

  preMustacheProcess(APISpecs, codeGenNameConventions, enumMetadataMap) {
    let csharpBaseType;

    // Process APISpecs for CSharp
    // 1. type normalization
    // 2. enum type naming and referencing
    for (const clsName in APISpecs) {
      const APIClsSpec = APISpecs[clsName];
      for (const index in APIClsSpec.apis) {
        const APISpec = APIClsSpec.apis[index];
        for (const index2 in APISpec.params) {
          const paramSpec = APISpec.params[index2];
          if (paramSpec.name == 'params') {
            paramSpec.param_name_params = true;
          }
          if (['file', 'bytes', 'zipbytes'].indexOf(paramSpec.name) != -1) {
            APISpec.params[index2] = undefined;
            APISpec.allow_file_upload = true;
            continue;
          }
          if (paramSpec.type) {
            const baseType = getBaseType(paramSpec.type);
            if (enumMetadataMap[baseType]) {
              paramSpec.is_enum_param = true;
              const metadata = enumMetadataMap[baseType];
              if (!metadata.node) {
                if (!APIClsSpec.api_spec_based_enum_reference) {
                  APIClsSpec.api_spec_based_enum_reference = [];
                  APIClsSpec.api_spec_based_enum_list = {};
                }
                if (!APIClsSpec.api_spec_based_enum_list[metadata.field_or_param]) {
                  APIClsSpec.api_spec_based_enum_reference.push(metadata);
                  APIClsSpec.api_spec_based_enum_list[metadata.field_or_param] = true;
                }
                csharpBaseType = 'Enum' + metadata['field_or_param:pascal_case'];
              } else {
                csharpBaseType = metadata.node + '.Enum' + metadata['field_or_param:pascal_case'];
              }
              paramSpec['type:csharp'] = this.getTypeForCSharp(paramSpec.type.replace(baseType, csharpBaseType));
              paramSpec['basetype:csharp'] = csharpBaseType;
            } else {
              paramSpec['type:csharp'] = getTypeForCSharp(paramSpec.type);
              if (paramSpec['type:csharp'] == 'string') {
                paramSpec.is_string = true;
              }
            }
          }
        }
        if ((APISpec['api:name:underscore'] == 'update' && APISpec['method'] == 'POST') || (APISpec['api:name:underscore'] == 'delete' && APISpec['method'] == 'DELETE') ) {
          APISpec['is_update_or_delete'] = true;
        }
        if (APISpec.params) {
          APISpec.params = APISpec.params.filter(element => {
            return element != null;
          });
        }
      }

      for (const index in APIClsSpec.fields) {
        const fieldSpec = APIClsSpec.fields[index];
        const fieldCls = APISpecs[fieldSpec.type];
        if (fieldCls && fieldCls.has_get && fieldCls.has_id) {
          fieldSpec.is_root_node = true;
        }
        if (fieldSpec.type) {
          if (enumMetadataMap[fieldSpec.type]) {
            fieldSpec.is_enum_field = true;
          }
          const baseType = getBaseType(fieldSpec.type);
          if (APISpecs[baseType]) {
            fieldSpec.is_node = true;
            fieldSpec['csharp:node_base_type'] = getTypeForCSharp(baseType);
          }
          if (enumMetadataMap[baseType]) {
            const metadata = enumMetadataMap[baseType];
            csharpBaseType = 'Enum' + metadata['field_or_param:pascal_case'];
            fieldSpec['type:csharp'] = this.getTypeForCSharp(fieldSpec.type.replace(baseType, csharpBaseType));
            if (!APIClsSpec.api_spec_based_enum_reference) {
              APIClsSpec.api_spec_based_enum_reference = [];
              APIClsSpec.api_spec_based_enum_list = {};
            }
            if (!APIClsSpec.api_spec_based_enum_list[metadata.field_or_param]) {
              APIClsSpec.api_spec_based_enum_reference.push(metadata);
              APIClsSpec.api_spec_based_enum_list[metadata.field_or_param] = true;
            }
          } else {
            if (fieldSpec.keyvalue) {
              fieldSpec['type:csharp'] = 'List<KeyValue>';
            } else {
              fieldSpec['type:csharp'] = this.getTypeForCSharp(fieldSpec.type);
            }
          }
        }
      }
    }
    return APISpecs;
  },
  getTypeForCSharp: getTypeForCSharp,
  keywords: ['try', 'private', 'public', 'new', 'default', 'class']
};

exports.default = CodeGenLanguageCSharp;