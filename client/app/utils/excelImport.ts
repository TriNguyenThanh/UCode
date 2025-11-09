import * as XLSX from 'xlsx'

export interface ExcelColumn {
  key: string
  label: string
  required?: boolean
}

export interface ExcelImportOptions {
  columns: ExcelColumn[]
  validateRow?: (row: any, rowIndex: number) => string | null // Return error message or null if valid
}

export interface ExcelImportResult<T = any> {
  success: boolean
  data: T[]
  errors: string[]
  warnings: string[]
}

/**
 * Generic Excel file importer
 * @param file - Excel file to import
 * @param options - Import options including column definitions
 * @returns Promise with import result containing data and errors
 */
export async function importExcelFile<T = any>(
  file: File,
  options: ExcelImportOptions
): Promise<ExcelImportResult<T>> {
  const errors: string[] = []
  const warnings: string[] = []
  const data: T[] = []

  try {
    // Validate file type
    const validExtensions = ['.xlsx', '.xls', '.csv']
    const fileExtension = file.name.toLowerCase().slice(file.name.lastIndexOf('.'))
    
    if (!validExtensions.includes(fileExtension)) {
      return {
        success: false,
        data: [],
        errors: [`File không hợp lệ. Vui lòng chọn file Excel (.xlsx, .xls) hoặc CSV.`],
        warnings: [],
      }
    }

    // Read file
    const arrayBuffer = await file.arrayBuffer()
    const workbook = XLSX.read(arrayBuffer, { type: 'array' })

    // Get first sheet
    const firstSheetName = workbook.SheetNames[0]
    if (!firstSheetName) {
      return {
        success: false,
        data: [],
        errors: ['File Excel không có sheet nào.'],
        warnings: [],
      }
    }

    const worksheet = workbook.Sheets[firstSheetName]
    const jsonData = XLSX.utils.sheet_to_json(worksheet, { 
      header: 1,
      defval: '', 
      blankrows: false,
      raw: false // Read all data as text to preserve '0' values
    }) as any[][]

    if (jsonData.length === 0) {
      return {
        success: false,
        data: [],
        errors: ['File Excel không có dữ liệu.'],
        warnings: [],
      }
    }

    // Validate headers
    const headerRow = jsonData[0] as string[]
    const columnMap = new Map<number, string>()

    // Map columns by matching headers
    options.columns.forEach(col => {
      const headerIndex = headerRow.findIndex(
        h => h && h.toString().trim().toLowerCase() === col.label.toLowerCase()
      )
      
      if (headerIndex !== -1) {
        columnMap.set(headerIndex, col.key)
      } else if (col.required) {
        errors.push(`Không tìm thấy cột bắt buộc: "${col.label}"`)
      }
    })

    // Check if we have at least one column mapped
    if (columnMap.size === 0) {
      errors.push(`Không tìm thấy cột nào khớp với định dạng yêu cầu: ${options.columns.map(c => c.label).join(', ')}`)
    }

    // If there are header errors, return early
    if (errors.length > 0) {
      return {
        success: false,
        data: [],
        errors,
        warnings: [],
      }
    }

    // Process data rows (skip header)
    for (let i = 1; i < jsonData.length; i++) {
      const row = jsonData[i]
      const rowData: any = {}
      let hasData = false

      // Map columns to object - Convert all values to string to handle '0' correctly
      columnMap.forEach((key, colIndex) => {
        const value = row[colIndex]
        // Check if value exists (including '0', 0, false)
        if (value !== undefined && value !== null) {
          const stringValue = value.toString().trim()
          if (stringValue !== '') {
            hasData = true
            rowData[key] = stringValue
          } else {
            rowData[key] = ''
          }
        } else {
          rowData[key] = ''
        }
      })

      // Skip completely empty rows
      if (!hasData) {
        continue
      }

      // Validate required fields
      const requiredColumns = options.columns.filter(c => c.required)
      const missingFields = requiredColumns
        .filter(col => !rowData[col.key] || rowData[col.key].trim() === '')
        .map(col => col.label)

      if (missingFields.length > 0) {
        warnings.push(`Dòng ${i + 1}: Thiếu dữ liệu bắt buộc cho cột: ${missingFields.join(', ')}`)
        continue
      }

      // Custom validation
      if (options.validateRow) {
        const validationError = options.validateRow(rowData, i + 1)
        if (validationError) {
          warnings.push(`Dòng ${i + 1}: ${validationError}`)
          continue
        }
      }

      data.push(rowData as T)
    }

    if (data.length === 0 && warnings.length > 0) {
      return {
        success: false,
        data: [],
        errors: ['Không có dòng dữ liệu hợp lệ nào được tìm thấy.'],
        warnings,
      }
    }

    return {
      success: true,
      data,
      errors,
      warnings,
    }
  } catch (error: any) {
    return {
      success: false,
      data: [],
      errors: [`Lỗi khi đọc file: ${error.message || 'Unknown error'}`],
      warnings,
    }
  }
}

/**
 * Generate a sample Excel file template
 * @param columns - Column definitions
 * @param filename - Output filename
 */
export function downloadExcelTemplate(columns: ExcelColumn[], filename: string = 'template.xlsx') {
  // Create header row
  const headerRow = columns.map(col => col.label)
  
  // Create a sample data row (optional)
  const sampleRow = columns.map(col => `Mẫu ${col.label}`)

  // Create worksheet
  const worksheet = XLSX.utils.aoa_to_sheet([headerRow, sampleRow])

  // Auto-size columns
  const colWidths = columns.map(col => ({ wch: Math.max(col.label.length + 5, 20) }))
  worksheet['!cols'] = colWidths

  // Create workbook
  const workbook = XLSX.utils.book_new()
  XLSX.utils.book_append_sheet(workbook, worksheet, 'Template')

  // Download
  XLSX.writeFile(workbook, filename)
}
