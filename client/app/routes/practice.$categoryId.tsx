import * as React from 'react'
import { useLoaderData, redirect, Link } from 'react-router'
import type { Route } from './+types/practice.$categoryId'
import { auth } from '~/auth'
import { Navigation } from '~/components/Navigation'
import {
  Container,
  Typography,
  Box,
  Card,
  CardContent,
  CardActionArea,
  Chip,
  IconButton,
  Paper,
} from '@mui/material'
import ArrowBackIcon from '@mui/icons-material/ArrowBack'
import CodeIcon from '@mui/icons-material/Code'
import AccessTimeIcon from '@mui/icons-material/AccessTime'
import { mockPracticeCategories, mockProblems } from '~/data/mock'

export const meta: Route.MetaFunction = () => [
  { title: 'Danh mục luyện tập | UCode' },
  { name: 'description', content: 'Danh sách bài tập trong danh mục.' },
]

export async function clientLoader({ params }: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')

  const category = mockPracticeCategories.find((c) => c.id === params.categoryId)
  if (!category) throw new Response('Not Found', { status: 404 })

  // Filter problems by category
  const problems = mockProblems.filter((p) => p.category === category.name)

  return { user, category, problems }
}

export default function PracticeCategoryDetail() {
  const { category, problems } = useLoaderData<typeof clientLoader>()

  const getDifficultyColor = (difficulty: string) => {
    switch (difficulty) {
      case 'Easy':
        return 'success'
      case 'Medium':
        return 'warning'
      case 'Hard':
        return 'error'
      default:
        return 'default'
    }
  }

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <Navigation />

      <Container maxWidth='xl' sx={{ py: 4 }}>
        {/* Back Button */}
        <IconButton component={Link} to='/practice' sx={{ mb: 2 }}>
          <ArrowBackIcon />
        </IconButton>

        {/* Category Header */}
        <Paper
          sx={{
            mb: 4,
            overflow: 'hidden',
            background: 'linear-gradient(135deg, #FACB01 0%, #ffd54f 100%)',
          }}
        >
          <Box sx={{ p: 4 }}>
            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
              <Typography variant='h2' sx={{ mr: 2 }}>
                {category.icon}
              </Typography>
              <Typography variant='h3' sx={{ fontWeight: 700, color: 'secondary.main' }}>
                {category.name}
              </Typography>
            </Box>
            <Typography variant='body1' sx={{ color: 'secondary.main', mb: 2, opacity: 0.9 }}>
              {category.description}
            </Typography>
            <Chip
              label={`${category.problemCount} bài tập`}
              sx={{
                bgcolor: 'secondary.main',
                color: 'primary.main',
                fontWeight: 600,
              }}
            />
          </Box>
        </Paper>

        {/* Problems List */}
        <Box>
          <Typography variant='h5' sx={{ fontWeight: 600, mb: 3, display: 'flex', alignItems: 'center', gap: 1 }}>
            <CodeIcon sx={{ color: 'primary.main' }} />
            Danh sách bài tập ({problems.length})
          </Typography>

          {problems.length === 0 ? (
            <Paper sx={{ p: 4, textAlign: 'center' }}>
              <Typography color='text.secondary'>Chưa có bài tập nào trong danh mục này 📝</Typography>
            </Paper>
          ) : (
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
              {problems.map((problem, index) => (
                <Card
                  key={problem.id}
                  elevation={0}
                  sx={{
                    border: '2px solid',
                    borderColor: 'divider',
                    transition: 'all 0.2s',
                    '&:hover': {
                      borderColor: 'primary.main',
                      transform: 'translateY(-2px)',
                      boxShadow: 3,
                    },
                  }}
                >
                  <CardActionArea component={Link} to={`/problem/${problem.id}`}>
                    <CardContent sx={{ p: 3 }}>
                      <Box sx={{ display: 'flex', gap: 3 }}>
                        {/* Number Badge */}
                        <Box
                          sx={{
                            width: 50,
                            height: 50,
                            borderRadius: '50%',
                            bgcolor: 'primary.main',
                            display: 'flex',
                            alignItems: 'center',
                            justifyContent: 'center',
                            flexShrink: 0,
                          }}
                        >
                          <Typography variant='h6' sx={{ fontWeight: 700, color: 'secondary.main' }}>
                            {index + 1}
                          </Typography>
                        </Box>

                        {/* Content */}
                        <Box sx={{ flexGrow: 1 }}>
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 2, mb: 1 }}>
                            <Typography variant='h6' sx={{ fontWeight: 600 }}>
                              {problem.title}
                            </Typography>
                            <Chip
                              label={problem.difficulty}
                              size='small'
                              color={getDifficultyColor(problem.difficulty) as any}
                            />
                          </Box>

                          <Typography variant='body2' color='text.secondary' sx={{ mb: 2 }}>
                            {problem.description}
                          </Typography>

                          {/* Tags */}
                          <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flexWrap: 'wrap' }}>
                            {problem.tags.map((tag) => (
                              <Chip key={tag} label={tag} size='small' variant='outlined' />
                            ))}
                            <Chip
                              label={`${problem.timeLimit}s / ${problem.memoryLimit}MB`}
                              size='small'
                              icon={<AccessTimeIcon />}
                            />
                          </Box>
                        </Box>
                      </Box>
                    </CardContent>
                  </CardActionArea>
                </Card>
              ))}
            </Box>
          )}
        </Box>
      </Container>
    </Box>
  )
}
