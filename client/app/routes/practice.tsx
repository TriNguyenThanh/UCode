import * as React from 'react'
import { useLoaderData, redirect, Link } from 'react-router'
import type { Route } from './+types/practice'
import { auth } from '~/auth'
import { Navigation } from '~/components/Navigation'
import { Container, Typography, Box, Card, CardContent, CardActionArea, Chip } from '@mui/material'
import CodeIcon from '@mui/icons-material/Code'
import { mockPracticeCategories } from '~/data/mock'

export const meta: Route.MetaFunction = () => [
  { title: 'Luyện tập | UCode' },
  { name: 'description', content: 'Danh sách bài tập luyện tập theo chủ đề.' },
]

export async function clientLoader({}: Route.ClientLoaderArgs) {
  const user = auth.getUser()
  if (!user) throw redirect('/login')

  return { user, categories: mockPracticeCategories }
}

export default function Practice() {
  const { categories } = useLoaderData<typeof clientLoader>()

  return (
    <Box sx={{ minHeight: '100vh', bgcolor: 'grey.50' }}>
      <Navigation />

      <Container maxWidth='xl' sx={{ py: 4 }}>
        {/* Header */}
        <Box sx={{ mb: 4 }}>
          <Typography variant='h4' sx={{ fontWeight: 700, mb: 1, display: 'flex', alignItems: 'center', gap: 1 }}>
            <CodeIcon sx={{ color: 'primary.main', fontSize: 36 }} />
            Luyện tập
          </Typography>
          <Typography variant='body1' color='text.secondary'>
            Rèn luyện kỹ năng lập trình với các bài tập theo chủ đề
          </Typography>
        </Box>

        {/* Categories Grid */}
        <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', md: '1fr 1fr 1fr' }, gap: 3 }}>
          {categories.map((category) => (
            <Card
              key={category.id}
              elevation={0}
              sx={{
                border: '2px solid',
                borderColor: 'divider',
                height: '100%',
                transition: 'all 0.2s',
                '&:hover': {
                  borderColor: 'primary.main',
                  transform: 'translateY(-4px)',
                  boxShadow: 4,
                },
              }}
            >
              <CardActionArea component={Link} to={`/practice/${category.id}`} sx={{ height: '100%' }}>
                <CardContent sx={{ height: '100%', display: 'flex', flexDirection: 'column', p: 3 }}>
                  <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                    <Typography variant='h2' sx={{ mr: 2 }}>
                      {category.icon}
                    </Typography>
                    <Typography variant='h5' sx={{ fontWeight: 600 }}>
                      {category.name}
                    </Typography>
                  </Box>

                  <Typography variant='body1' color='text.secondary' sx={{ mb: 3, flexGrow: 1 }}>
                    {category.description}
                  </Typography>

                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                    <Chip
                      label={`${category.problemCount} bài`}
                      sx={{
                        bgcolor: 'primary.main',
                        color: 'secondary.main',
                        fontWeight: 600,
                      }}
                    />
                    <Chip label='Miễn phí' size='small' color='success' variant='outlined' />
                  </Box>
                </CardContent>
              </CardActionArea>
            </Card>
          ))}
        </Box>
      </Container>
    </Box>
  )
}
