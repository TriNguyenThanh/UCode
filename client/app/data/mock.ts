import type { Class, Assignment, Problem, PracticeCategory } from '~/types/index'

export const mockClasses: Class[] = [
  {
    id: '1',
    name: 'Cấu trúc dữ liệu và Giải thuật',
    code: 'CS201',
    teacherName: 'TS. Nguyễn Văn A',
    semester: 'HK1 2024-2025',
    description: 'Học về các cấu trúc dữ liệu cơ bản và giải thuật',
    studentCount: 45,
  },
  {
    id: '2',
    name: 'Lập trình hướng đối tượng',
    code: 'CS202',
    teacherName: 'ThS. Trần Thị B',
    semester: 'HK1 2024-2025',
    description: 'Học về OOP với Java',
    studentCount: 38,
  },
  {
    id: '3',
    name: 'Cơ sở dữ liệu',
    code: 'CS203',
    teacherName: 'TS. Lê Văn C',
    semester: 'HK1 2024-2025',
    description: 'SQL và thiết kế CSDL',
    studentCount: 42,
  },
]

export const mockProblems: Problem[] = [
  {
    id: '1',
    title: 'Two Sum',
    difficulty: 'Easy',
    description: 'Tìm 2 số trong mảng có tổng bằng target',
    category: 'Array',
    tags: ['Array', 'Hash Table'],
    timeLimit: 1,
    memoryLimit: 128,
    testCases: [],
    sampleInput: '[2,7,11,15], target = 9',
    sampleOutput: '[0,1]',
  },
  {
    id: '2',
    title: 'Binary Search',
    difficulty: 'Easy',
    description: 'Tìm kiếm nhị phân trong mảng đã sắp xếp',
    category: 'Binary Search',
    tags: ['Binary Search', 'Array'],
    timeLimit: 1,
    memoryLimit: 128,
    testCases: [],
  },
  {
    id: '3',
    title: 'Merge Sort',
    difficulty: 'Medium',
    description: 'Cài đặt thuật toán sắp xếp trộn',
    category: 'Sorting',
    tags: ['Sorting', 'Divide and Conquer'],
    timeLimit: 2,
    memoryLimit: 256,
    testCases: [],
  },
  {
    id: '4',
    title: 'Linked List Cycle',
    difficulty: 'Medium',
    description: 'Kiểm tra xem linked list có chu trình hay không',
    category: 'Linked List',
    tags: ['Linked List', 'Two Pointers'],
    timeLimit: 1,
    memoryLimit: 128,
    testCases: [],
  },
]

export const mockAssignments: Assignment[] = [
  {
    id: '1',
    classId: '1',
    className: 'Cấu trúc dữ liệu và Giải thuật',
    title: 'Bài tập tuần 1: Array và Hash Table',
    description: 'Làm quen với Array và Hash Table cơ bản',
    dueDate: new Date(Date.now() + 2 * 24 * 60 * 60 * 1000), // 2 ngày nữa
    startDate: new Date(Date.now() - 7 * 24 * 60 * 60 * 1000),
    problems: ['1', '2'], // Array of problem IDs
    totalPoints: 100,
    status: 'active',
  },
  {
    id: '2',
    classId: '1',
    className: 'Cấu trúc dữ liệu và Giải thuật',
    title: 'Bài tập tuần 2: Sorting Algorithms',
    description: 'Thực hành các thuật toán sắp xếp',
    dueDate: new Date(Date.now() + 5 * 24 * 60 * 60 * 1000), // 5 ngày nữa
    startDate: new Date(Date.now() - 2 * 24 * 60 * 60 * 1000),
    problems: ['3'], // Array of problem IDs
    totalPoints: 150,
    status: 'active',
  },
  {
    id: '3',
    classId: '2',
    className: 'Lập trình hướng đối tượng',
    title: 'Bài tập về Class và Object',
    description: 'Tạo các class cơ bản trong Java',
    dueDate: new Date(Date.now() + 4 * 24 * 60 * 60 * 1000), // 4 ngày nữa
    startDate: new Date(Date.now() - 3 * 24 * 60 * 60 * 1000),
    problems: ['4'], // Array of problem IDs
    totalPoints: 100,
    status: 'active',
  },
]

export const mockPracticeCategories: PracticeCategory[] = [
  {
    id: '1',
    name: 'Array',
    description: 'Các bài tập về mảng',
    problemCount: 45,
    icon: '📊',
  },
  {
    id: '2',
    name: 'String',
    description: 'Các bài tập về chuỗi',
    problemCount: 38,
    icon: '📝',
  },
  {
    id: '3',
    name: 'Linked List',
    description: 'Các bài tập về danh sách liên kết',
    problemCount: 25,
    icon: '🔗',
  },
  {
    id: '4',
    name: 'Binary Search',
    description: 'Tìm kiếm nhị phân',
    problemCount: 20,
    icon: '🔍',
  },
  {
    id: '5',
    name: 'Sorting',
    description: 'Các thuật toán sắp xếp',
    problemCount: 15,
    icon: '📈',
  },
  {
    id: '6',
    name: 'Dynamic Programming',
    description: 'Quy hoạch động',
    problemCount: 52,
    icon: '🧮',
  },
]
